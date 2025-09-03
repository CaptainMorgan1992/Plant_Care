using Auth0_Blazor.Services;
using Quartz;

namespace Auth0_Blazor.Jobs;

public class WateringNotificationJob : IJob
{
    private readonly NotificationService _notificationService;
    private readonly ReminderLogicService _reminderLogicService;
    private readonly ILogger<WateringNotificationJob> _logger;
    private readonly UserPlantService _userPlantService;
    
    public WateringNotificationJob (
        NotificationService notificationService,
        ReminderLogicService reminderLogicService,
        ILogger<WateringNotificationJob> logger,
        UserPlantService userPlantService)
    {
        _notificationService = notificationService;
        _reminderLogicService = reminderLogicService;
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
        /*var normalWateringPlants = await _reminderLogicService.FetchUserPlantsWithMediumWateringNeedsAsync();
    
        foreach (var plantName in normalWateringPlants)
        {
            _notificationService.ShowWateringNotification(plantName);
        }*/
        
        var groupedUsers = await _userPlantService.GetUsersWithPlantsGroupedByWaterFrequencyAsync();
        
        foreach(var frequencyGroup in groupedUsers)
        {
            var frequency = frequencyGroup.Key;
            var users = frequencyGroup.Value;
            
            foreach(var (user, plants) in users)
            {
                foreach(var plant in plants)
                {
                    _notificationService.ShowWateringNotification(user, plant.Name, frequency);
                }
            }
        }
    }
}