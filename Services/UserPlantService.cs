using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Services;

public class UserPlantService
{
    private readonly ApplicationDbContext _db;
    private readonly UserService _userService;
    private readonly ILogger<UserPlantService> _logger;

    public UserPlantService (ApplicationDbContext db, UserService userService, ILogger<UserPlantService> logger)
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

    /*public Task RemovePlantFromUserHouseholdAsync
    {
        
    }*/
}