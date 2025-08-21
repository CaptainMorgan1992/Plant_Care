using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Services;

public class PlantService
{
    private readonly ApplicationDbContext _context;

    public PlantService(ApplicationDbContext context)
    {
        _context = context;
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
}