using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services.IService;
using Microsoft.EntityFrameworkCore;


namespace Auth0_Blazor.Services;

public class PlantService : IPlantService
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;

    public PlantService(ApplicationDbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<List<Plant>> GetAllPlantsAsync()
    {
        return await _context.Plants.ToListAsync();
    }
    
    public async Task<Plant> GetPlantByIdAsync(int id)
    {
        var plant = await _context.Plants.FindAsync(id);

        if (plant == null)
        {
            throw new KeyNotFoundException($"Plant with ID {id} not found.");
        }
        
        return plant;
    }
    
    public async Task<bool> AddNewPlantAsync(Plant plant, string ownerId)
    {
        _userService.ValidateOwnerId(ownerId);
        
        var isUserAdmin = await _userService.IsUserAdminAsync(ownerId);

        if (!isUserAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can add new plants.");
        }
        _context.Plants.Add(plant);
        await _context.SaveChangesAsync();
        return true;
    }
    
}