using System.Security.Principal;
using Auth0_Blazor.Data;
using Auth0_Blazor.Enums;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services.IService;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Auth0_Blazor.Services;

public class UserPlantService : IUserPlantService
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    private readonly IUserService _userService;
    private readonly ILogger<UserPlantService> _logger;

    public UserPlantService (
        IDbContextFactory<ApplicationDbContext> factory,
        IUserService userService,
        ILogger<UserPlantService> logger)
    {
        _factory = factory;
        _userService = userService;
        _logger = logger;
    }
    
    public virtual async Task AddPlantToUserHouseholdAsync(int plantId)
    {
        await _userService.SaveUserOnClick();
        var ownerId = await _userService.GetUserAuth0IdAsync();
        var userId = await _userService.IsValidUserByOwnerIdAsync(ownerId);
        var validUserId = userId!;
 
        await AddPlantToUser(plantId, validUserId.Value);
        
    }
    
    public virtual async Task<List<UserPlant>> GetUserPlantsAsync()
    {
        var ownerId = await _userService.GetUserAuth0IdAsync();
        var userId = await _userService.IsValidUserByOwnerIdAsync(ownerId);
        var validUserId = userId!.Value;
        
        var userPlants =  await GetAllPlantsForUserById(validUserId);
        return userPlants;
    }
    
    public virtual async Task RemovePlantFromUserHouseholdAsync(int plantId)
    {
        var ownerId = await _userService.GetUserAuth0IdAsync();
        var userId = await _userService.IsValidUserByOwnerIdAsync(ownerId);
        var validUserId = userId!.Value;

        var userPlant = await DoesUserHavePlantAsync(plantId, validUserId);

        if (userPlant != null)
        {
            await using var db = _factory.CreateDbContext();
            db.UserPlants.Remove(userPlant);
            await db.SaveChangesAsync();
            _logger.LogInformation("PlantId {PlantId} has been deleted from user.", plantId);
        }
    }
    
    
    public virtual async Task<List<UserPlant>> GetTop6UserPlantsAsync()
    {
        // Get the most saved PlantIds (grouped and ordered by count)
        var topPlantIds = await FetchTopUserPlantIdsAsync();

        // Get UserPlant entries for these PlantIds (one per PlantId)
        var topUserPlants = await FetchTopUserPlantEntriesAsync(topPlantIds);

        if (topUserPlants.Count < 6)
        {
            var existingPlantIds = topUserPlants.Select(up => up.PlantId).ToHashSet();
            var remainingPlants = await FetchRemainingPlantsAsync(existingPlantIds);
            var randomPlants = RandomizeRemainingPlantsAsync(remainingPlants, topUserPlants);
            topUserPlants = CreateNewListOfUserPlants(topUserPlants, randomPlants);
        }

        return topUserPlants;
    }
    
    /*
     * Creates an empty dictionary where the WaterFrequency is the key, and the value is a list of tuples (User with their plants).
     * Fetches all users from the database.
     * For each user, fetches their associated plants and groups them by WaterFrequency.
     * If the group does not exist in the result dictionary, it initializes a new list for that frequency.
     * Adds the tuple (user, list of plants) to the appropriate frequency group in the result dictionary.
     * Returns the populated dictionary where each waterfrequency has a list of users + their plants with the appropriate frequency.
     */

    public virtual async Task<Dictionary<WaterFrequency, List<(User user, List<Plant> plants)>>>
        GetUsersWithPlantsGroupedByWaterFrequencyAsync()
    {
        var result = new Dictionary<WaterFrequency, List<(User user, List<Plant>)>>();
        
        await using var db = _factory.CreateDbContext();
        var allUsers = await db.Users.ToListAsync();
        
        foreach (var user in allUsers)
        {
            var userPlants = await GetAllPlantsForUserById(user.Id);
            var groupedPlants = GroupPlantsByWateringNeedsAndReturnDictionary(userPlants);
            
            result = AddUserGroupedPlantsToDictionary(result, groupedPlants, user);
        }
        return result;
    }
    
    public virtual Dictionary<WaterFrequency, List<(User, List<Plant>)>> 
        AddUserGroupedPlantsToDictionary(
            Dictionary<WaterFrequency, List<(User, List<Plant>)>> result,
            Dictionary<WaterFrequency, List<Plant>> groupedPlants,
            User user)
    {
        foreach (var kvp in groupedPlants)
        {
            if (!result.ContainsKey(kvp.Key))
                result[kvp.Key] = new List<(User, List<Plant>)>();

            result[kvp.Key].Add((user, kvp.Value));
        }
        return result;
    }
    
    public virtual async Task<List<int>> FetchTopUserPlantIdsAsync()
    {
        await using var db = _factory.CreateDbContext();
        return await db.UserPlants
            .GroupBy(up => up.PlantId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(6)
            .ToListAsync();
    }
    
    public virtual async Task<List<UserPlant>> FetchTopUserPlantEntriesAsync(List<int> topPlantIds)
    {
        await using var db = _factory.CreateDbContext();
        return await db.UserPlants
            .Where(up => topPlantIds.Contains(up.PlantId))
            .Include(up => up.Plant)
            .GroupBy(up => up.PlantId)
            .Select(g => g.First())
            .ToListAsync();
    }
    
    public virtual async Task AddPlantToUser(int plantId, int userId)
    {
        var userPlant = new UserPlant
        {
            PlantId = plantId,
            UserId = userId, 
        };

        await using var db = _factory.CreateDbContext();
        db.UserPlants.Add(userPlant);
        await db.SaveChangesAsync();
    }
    
    public virtual async Task <List<UserPlant>> GetAllPlantsForUserById(int validUserId)
    {
        await using var db = _factory.CreateDbContext();
        return await db.UserPlants
            .Where(up => up.UserId == validUserId)
            .Include(up => up.Plant)
            .ToListAsync();
    }
    
    public virtual async Task<UserPlant?> DoesUserHavePlantAsync(int plantId, int validUserId)
    {
        await using var db = _factory.CreateDbContext();
        var userPlant = await db.UserPlants
            .FirstOrDefaultAsync(up => up.UserId == validUserId && up.PlantId == plantId);

        if (userPlant != null)
            return userPlant;

        _logger.LogInformation("UserId {UserId} does not have PlantId {PlantId}.", validUserId, plantId);
        return null;
    }
    
    public virtual async Task<List<Plant>> FetchRemainingPlantsAsync(HashSet<int> existingPlantIds)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Plants
            .Where(p => !existingPlantIds.Contains(p.Id))
            .ToListAsync();
    }
    
    public virtual List<Plant> RandomizeRemainingPlantsAsync(List<Plant> remainingPlants, List<UserPlant> topUserPlants)
    {
        return remainingPlants
            .OrderBy(_ => Guid.NewGuid())
            .Take(6 - topUserPlants.Count)
            .ToList();
    }
    
    public virtual List<UserPlant> CreateNewListOfUserPlants(List<UserPlant> topUserPlants, List<Plant> randomPlants)
    {
        topUserPlants.AddRange(
            randomPlants.Select(plant => new UserPlant
            {
                PlantId = plant.Id,
                Plant = plant,
                UserId = 0
            })
        );

        return topUserPlants;
    }
    
    public virtual Dictionary<WaterFrequency, List<Plant>> GroupPlantsByWateringNeedsAndReturnDictionary(List<UserPlant> userPlants)
    {
        return userPlants
            .Where(up => up.Plant != null)
            .GroupBy(up => up.Plant!.WaterFrequency)
            .ToDictionary(
                g => g.Key,
                g => g.Select(up => up.Plant!).ToList());
    }
    
}
