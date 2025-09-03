using Auth0_Blazor.Data;
using Auth0_Blazor.Enums;
using Auth0_Blazor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Services;

public class UserPlantService
{
    private readonly ApplicationDbContext _db;
    private readonly UserService _userService;
    private readonly ILogger<UserPlantService> _logger;

    public UserPlantService (
        ApplicationDbContext db,
        UserService userService,
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
        var userId = await _userService.GetUserIdByOwnerIdAsync(ownerId);

        if (!userId.HasValue)
        {
            _logger.LogWarning("No user found for OwnerId {OwnerId}. Plant cannot be connected.", ownerId);
            return;
        }

        var exists = await _db.UserPlants.AnyAsync(up => up.PlantId == plantId && up.UserId == userId.Value);
        if (exists)
        {
            _logger.LogInformation("PlantId {PlantId} is already connected to {UserId}.", plantId, userId.Value);
            return;
        }

        var userPlant = new UserPlant
        {
            PlantId = plantId,
            UserId = userId.Value
        };

        _db.UserPlants.Add(userPlant);
        await _db.SaveChangesAsync();

        _logger.LogInformation("PlantId {PlantId} kopplades till UserId {UserId}.", plantId, userId.Value);
    }

    /* Kolla upp detta */
    public async Task<List<UserPlant>> GetAllUserPlantsByIdAsync(string auth0UserId)
    {
        _logger.LogInformation("Auth0 UserId from UserStateService: {Auth0UserId}", auth0UserId);
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.OwnerId == auth0UserId);

        if (user == null) return new List<UserPlant>(); 

        var internalUserId = user.Id;
        
        var userPlants = await _db.UserPlants
            .Include(up => up.Plant)
            .Where(up => up.UserId == internalUserId)
            .ToListAsync();

       
        return userPlants;
    }
    
    public async Task<List<UserPlant>> GetUserPlantsAsync()
    {
        var ownerId = await _userService.GetUserAuth0IdAsync();
        var userId = await _userService.GetUserIdByOwnerIdAsync(ownerId);

        if (!userId.HasValue)
        {
            _logger.LogWarning("Ingen användare hittades för OwnerId {OwnerId}. Inga växter kan hämtas.", ownerId);
            return new List<UserPlant>();
        }

        return await _db.UserPlants
            .Where(up => up.UserId == userId.Value)
            .Include(up => up.Plant)
            .ToListAsync();
    }
    
    public async Task<bool> UserHasPlantAsync(int plantId)
    {
        var ownerId = await _userService.GetUserAuth0IdAsync();
        var userId = await _userService.GetUserIdByOwnerIdAsync(ownerId);
        return await _db.UserPlants
            .AnyAsync(up => up.UserId == userId && up.PlantId == plantId);
    }

    public async Task RemovePlantFromUserHouseholdAsync(int plantId)
    {
        var ownerId = await _userService.GetUserAuth0IdAsync();
        var userId = await _userService.GetUserIdByOwnerIdAsync(ownerId);
        
        // Search for the first matching UserPlant entry.
        // This is a 'LINQ' and entity framework transfers this automaticlally to SQL.
        var userPlant = await _db.UserPlants
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PlantId == plantId);

        if (userPlant != null)
        {
            _db.UserPlants.Remove(userPlant);
            await _db.SaveChangesAsync();
            _logger.LogInformation("PlantId {PlantId} has been deleted from user.", plantId);
        }
        else
        {
            _logger.LogInformation("PlantId {PlantId} has not been deleted from user. UserPlant {userPlant} cannot be found.", plantId, userPlant);
        }
    }
    public async Task<List<UserPlant>> GetTop6UserPlantsAsync()
    {
        // Get the most saved PlantIds (grouped and ordered by count)
        var topPlantIds = await _db.UserPlants
            .GroupBy(up => up.PlantId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(6)
            .ToListAsync();

        // Get UserPlant entries for these PlantIds (one per PlantId)
        var topUserPlants = await _db.UserPlants
            .Where(up => topPlantIds.Contains(up.PlantId))
            .Include(up => up.Plant)
            .GroupBy(up => up.PlantId)
            .Select(g => g.First())
            .ToListAsync();

        if (topUserPlants.Count < 6)
        {
            var existingPlantIds = topUserPlants.Select(up => up.PlantId).ToHashSet();
            var remainingPlants = await _db.Plants
                .Where(p => !existingPlantIds.Contains(p.Id))
                .ToListAsync();

            var randomPlants = remainingPlants
                .OrderBy(_ => Guid.NewGuid())
                .Take(6 - topUserPlants.Count)
                .ToList();

            foreach (var plant in randomPlants)
            {
                topUserPlants.Add(new UserPlant
                {
                    PlantId = plant.Id,
                    Plant = plant,
                    UserId = 0
                });
            }
        }

        return topUserPlants;
    }
    
    /*public async Task<List<User>> GetAllUsersWithPlantsAsync(WaterFrequency frequency)
    {
        var usersWithPlants = await _db.Users
            .Where(u => _db.UserPlants
                .Any(up => up.UserId == u.Id && up.Plant.WaterFrequency == frequency))
            .ToListAsync();

        return usersWithPlants;
    }*/
    
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
            var userPlants = await _db.UserPlants
                .Where(up => up.UserId == user.Id)
                .Include(up => up.Plant)
                .ToListAsync();
            
            var groupedPlants = userPlants
                .Where(up => up.Plant != null)
                .GroupBy(up => up.Plant!.WaterFrequency)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(up => up.Plant!).ToList());

            foreach (var kvp in groupedPlants)
            {
                if (!result.ContainsKey(kvp.Key))
                    result[kvp.Key] = new List<(User, List<Plant>)>();

                result[kvp.Key].Add((user, kvp.Value));
            }
        }
        _logger.LogInformation("Result from GetUsersWithPlantsGroupedByWaterFrequencyAsync: {Result}", result);
        return result;
    }
}