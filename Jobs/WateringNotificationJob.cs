using Auth0_Blazor.Enums;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services;
using Auth0_Blazor.Services.IService;
using Quartz;

namespace Auth0_Blazor.Jobs;

public class WateringNotificationJob : IJob
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<WateringNotificationJob> _logger;
    private readonly IUserPlantService _userPlantService;
    
    public WateringNotificationJob (
        INotificationService notificationService,
        ILogger<WateringNotificationJob> logger,
        IUserPlantService userPlantService)
    {
        _notificationService = notificationService;
        _logger = logger;
        _userPlantService = userPlantService;
    }
    
    /*
     * Fetches the dictionary of users with plants grouped by watering frequency
     * Loops through each and every watering frequency group (Low, Normal, High)
     * For each user in the group, it loops through their plants and sends a notification for each plant
     */
    public async Task Execute(IJobExecutionContext context)
    {
        // Fetch Frequency from JobDataMap
        var frequencyString = context.JobDetail.JobDataMap.GetString("frequency");
        if (!Enum.TryParse<WaterFrequency>(frequencyString, out var frequency))
        {
            _logger.LogWarning("Not a valid frequency: {frequency}", frequency);
            return;
        }
        
        // Fetches a dictionary where the key is the WaterFrequency and the value is a list of tuples (User, List<Plant>)
        var groupedUsers = await _userPlantService.GetUsersWithPlantsGroupedByWaterFrequencyAsync();
        
        // Fetches a group for each watering frequency
        var users = groupedUsers.GetValueOrDefault(frequency) ?? new List<(User, List<Plant>)>();

        // Loops through each user in the group. For each user, loops through their plants and sends a notification for each plant.
        foreach(var (user, plants) in users)
        {
            foreach(var plant in plants)
            {
                _notificationService.ShowWateringNotification(user, plant.Name);
            }
        }
    }
}