using Auth0_Blazor.Enums;
using Auth0_Blazor.Models;

namespace Auth0_Blazor.Services;

public class ReminderLogicService
{
    private readonly UserPlantService _userPlantService;
    private List<string> _normalWateringPlantNames = [];
    private List<string> _highWateringPlantNames = [];
    private List<string> _lowWateringPlantNames = [];
    
    public ReminderLogicService (UserPlantService userPlantService)
    {
        _userPlantService = userPlantService ?? throw new ArgumentNullException(nameof(userPlantService));
    }
    
    // Here, we would implement the logic to determine which plants needs medium watering
    public async Task<List<string>> FetchUserPlantsWithMediumWateringNeedsAsync()
    {
        var waterFrequency = WaterFrequency.Normal;
        _normalWateringPlantNames = await FetchPlantsForWatering(waterFrequency);
        return _normalWateringPlantNames;
    }

    public async Task<List<string>> FetchPlantsForHighWateringNeedsAsync()
    {
        var waterFrequency = WaterFrequency.High;
        _highWateringPlantNames = await FetchPlantsForWatering(waterFrequency);
        return _highWateringPlantNames;
    }
    
    public async Task <List<string>> FetchPlantsForLowWateringNeedsAsync()
    {
        var waterFrequency = WaterFrequency.Low;
        _lowWateringPlantNames = await FetchPlantsForWatering(waterFrequency);
        return _lowWateringPlantNames;
    }

    private async Task<List<string>> FetchPlantsForWatering( WaterFrequency frequency)
    {
        var userPlants = await _userPlantService.GetUserPlantsAsync();
        var plantNames = userPlants.Where(up => up.Plant != null && up.Plant.WaterFrequency == frequency)
            .Select(up => up.Plant!.Name)
            .ToList();

        return plantNames;
    }

}