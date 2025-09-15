using Auth0_Blazor.Enums;
using Auth0_Blazor.Models;

namespace Auth0_Blazor.Services.IService;

public interface IUserPlantService
{
    Task AddPlantToUserHouseholdAsync(int plantId);
    Task <List<UserPlant>> GetUserPlantsAsync();
    
    Task RemovePlantFromUserHouseholdAsync(int plantId);

    Task<List<UserPlant>> GetTop6UserPlantsAsync();

    Task<Dictionary<WaterFrequency, List<(User user, List<Plant> plants)>>>
        GetUsersWithPlantsGroupedByWaterFrequencyAsync();

    Dictionary<WaterFrequency, List<(User, List<Plant>)>> AddUserGroupedPlantsToDictionary(
        Dictionary<WaterFrequency, List<(User, List<Plant>)>> result,
        Dictionary<WaterFrequency, List<Plant>> groupedPlants,
        User user);
    
    Task <List<int>> FetchTopUserPlantIdsAsync();

    Task<List<UserPlant>> FetchTopUserPlantEntriesAsync(List<int> topPlantIds);
    
    Task AddPlantToUser(int plantId, int userId);
    
    Task<bool> PlantAlreadyAdded(int userId, int plantId);
    
    void DoesUserIdHaveValue(int? userId);
    
    Task<List<UserPlant>> GetAllPlantsForUserById(int userId);

    Task<UserPlant?> DoesUserHavePlantAsync(int plantId, int validUserId);

    Task<List<Plant>> FetchRemainingPlantsAsync(HashSet<int> existingPlantIds);
    List<Plant> RandomizeRemainingPlantsAsync(List<Plant> remainingPlants, List<UserPlant> topUserPlants);
    
    List<UserPlant> CreateNewListOfUserPlants(List<UserPlant> topUserPlants, List<Plant> randomPlants);

    Dictionary<WaterFrequency, List<Plant>> GroupPlantsByWateringNeedsAndReturnDictionary(List<UserPlant> userPlants);


}