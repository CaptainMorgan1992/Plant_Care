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
    private readonly ApplicationDbContext _db;
    private readonly IUserService _userService;
    private readonly ILogger<UserPlantService> _logger;

    public UserPlantService (
        ApplicationDbContext db,
        IUserService userService,
        ILogger<UserPlantService> logger)
    {
        _db = db;
        _userService = userService;
        _logger = logger;
    }
    
    public async Task AddPlantToUserHouseholdAsync(int plantId)
    {
        await _userService.SaveUserOnClick();
        var ownerId = await _userService.GetUserAuth0IdAsync();
        _userService.ValidateOwnerId(ownerId);
        var validOwnerId = ownerId!;
        var userId = await _userService.GetUserIdByOwnerIdAsync(validOwnerId);
        _userService.DoesUserIdHaveIntValue(userId);
        var validUserId = userId!;

        if (await PlantAlreadyAdded(validUserId.Value, plantId))
        {
            _logger.LogInformation("PlantId {PlantId} is already connected to {UserId}.", plantId, validUserId.Value);
            return;
        }
 
        await AddPlantToUser(plantId, validUserId.Value);
        
    }
    
    public async Task<List<UserPlant>> GetUserPlantsAsync()
    {
        var ownerId = await _userService.GetUserAuth0IdAsync();
        _userService.ValidateOwnerId(ownerId);
        var validOwnerId = ownerId!;
        var userId = await _userService.GetUserIdByOwnerIdAsync(validOwnerId);
        _userService.DoesUserIdHaveIntValue(userId);
        var validUserId = userId!.Value;
        var userPlants =  await GetAllPlantsForUserById(validUserId);
        return userPlants;
    }
    
    public async Task RemovePlantFromUserHouseholdAsync(int plantId)
    {
        var ownerId = await _userService.GetUserAuth0IdAsync();
        _userService.ValidateOwnerId(ownerId);
        var validOwnerId = ownerId!;
        var userId = await _userService.GetUserIdByOwnerIdAsync(validOwnerId);
        _userService.DoesUserIdHaveIntValue(userId);
        var validUserId = userId!.Value;

        var userPlant = await DoesUserHavePlantAsync(plantId, validUserId);

        if (userPlant != null)
        {
            _db.UserPlants.Remove(userPlant);
            await _db.SaveChangesAsync();
            _logger.LogInformation("PlantId {PlantId} has been deleted from user.", plantId);
        }
    }
    
    
    public async Task<List<UserPlant>> GetTop6UserPlantsAsync()
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

    public async Task<Dictionary<WaterFrequency, List<(User user, List<Plant> plants)>>>
        GetUsersWithPlantsGroupedByWaterFrequencyAsync()
    {
        var result = new Dictionary<WaterFrequency, List<(User user, List<Plant>)>>();
        
        var allUsers = await _db.Users.ToListAsync();
        
        foreach (var user in allUsers)
        {
            var userPlants = await GetAllPlantsForUserById(user.Id);
            var groupedPlants = GroupPlantsByWateringNeedsAndReturnDictionary(userPlants);
            
            result = AddUserGroupedPlantsToDictionary(result, groupedPlants, user);
        }
        return result;
    }
    
    public Dictionary<WaterFrequency, List<(User, List<Plant>)>> 
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
    
    public async Task<List<int>> FetchTopUserPlantIdsAsync()
    {
        return await _db.UserPlants
            .GroupBy(up => up.PlantId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(6)
            .ToListAsync();
    }
    
    public async Task<List<UserPlant>> FetchTopUserPlantEntriesAsync(List<int> topPlantIds)
    {
        return await _db.UserPlants
            .Where(up => topPlantIds.Contains(up.PlantId))
            .Include(up => up.Plant)
            .GroupBy(up => up.PlantId)
            .Select(g => g.First())
            .ToListAsync();
    }
    public async Task AddPlantToUser(int plantId, int userId)
    {
        var userPlant = new UserPlant
        {
            PlantId = plantId,
            UserId = userId
        };

        _db.UserPlants.Add(userPlant);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> PlantAlreadyAdded(int userId, int plantId)
    {
        return await _db.UserPlants.AnyAsync(up => up.PlantId == plantId && up.UserId == userId);
    }
    
    public async Task <List<UserPlant>> GetAllPlantsForUserById(int validUserId)
    {
        return await _db.UserPlants
            .Where(up => up.UserId == validUserId)
            .Include(up => up.Plant)
            .ToListAsync();
    }
    
    public async Task<UserPlant?> DoesUserHavePlantAsync(int plantId, int validUserId)
    {
        var userPlant = await _db.UserPlants
            .FirstOrDefaultAsync(up => up.UserId == validUserId && up.PlantId == plantId);

        if (userPlant != null)
            return userPlant;

        _logger.LogInformation("UserId {UserId} does not have PlantId {PlantId}.", validUserId, plantId);
        return null;
    }
    
    public async Task<List<Plant>> FetchRemainingPlantsAsync(HashSet<int> existingPlantIds)
    {
        return await _db.Plants
            .Where(p => !existingPlantIds.Contains(p.Id))
            .ToListAsync();
    }
    
    public List<Plant> RandomizeRemainingPlantsAsync(List<Plant> remainingPlants, List<UserPlant> topUserPlants)
    {
        return remainingPlants
            .OrderBy(_ => Guid.NewGuid())
            .Take(6 - topUserPlants.Count)
            .ToList();
    }
    
    public List<UserPlant> CreateNewListOfUserPlants(List<UserPlant> topUserPlants, List<Plant> randomPlants)
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
    
    public Dictionary<WaterFrequency, List<Plant>> GroupPlantsByWateringNeedsAndReturnDictionary(List<UserPlant> userPlants)
    {
        return userPlants
            .Where(up => up.Plant != null)
            .GroupBy(up => up.Plant!.WaterFrequency)
            .ToDictionary(
                g => g.Key,
                g => g.Select(up => up.Plant!).ToList());
    }
    
}
