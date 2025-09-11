namespace Auth0_Blazor.Services.IService;

public interface IUserPlantService
{
    Task AddPlantToUserHouseholdAsync(int plantId);
}