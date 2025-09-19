using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services.IService;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Services;

public class PlantService : IPlantService
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    private readonly UserService _userService;

    public PlantService(IDbContextFactory<ApplicationDbContext> factory, UserService userService)
    {
        _factory = factory;
        _userService = userService;
    }

    public async Task<List<Plant>> GetAllPlantsAsync()
    {
        await using var db = _factory.CreateDbContext();
        return await db.Plants.ToListAsync();
    }
    
    public async Task<Plant> GetPlantByIdAsync(int id)
    {
        await using var db = _factory.CreateDbContext();
        var plant = await db.Plants.FindAsync(id);
        return plant ?? throw new KeyNotFoundException($"Plant with ID {id} not found.");
    }
    
    public async Task<bool> AddNewPlantAsync(Plant plant, string ownerId)
    {
        _userService.ValidateOwnerId(ownerId);
        
        var isUserAdmin = await _userService.IsUserAdminAsync(ownerId);

        if (!isUserAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can add new plants.");
        }
        
        await using var db = _factory.CreateDbContext();
        db.Plants.Add(plant);
        await db.SaveChangesAsync();
        return true;
    }
    
}