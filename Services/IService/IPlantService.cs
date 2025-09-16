using Auth0_Blazor.Models;

namespace Auth0_Blazor.Services.IService;

public interface IPlantService
{
    Task<List<Plant>> GetAllPlantsAsync();
    
    Task<Plant> GetPlantByIdAsync(int id);

    Task<bool> AddNewPlantAsync(Plant plant, string ownerId);

}