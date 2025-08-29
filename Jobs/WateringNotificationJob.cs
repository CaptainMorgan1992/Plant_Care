using Auth0_Blazor.Services;
using Quartz;

namespace Auth0_Blazor.Jobs;

public class WateringNotificationJob : IJob
{
    
    private readonly NotificationService _notificationService;
    private readonly ReminderLogicService _reminderLogicService;
    
    public WateringNotificationJob (NotificationService notificationService, ReminderLogicService reminderLogicService)
    {
        _notificationService = notificationService;
        _reminderLogicService = reminderLogicService;
    }
    
    // Here, we would implement the logic to check which plants need watering
    public async Task Execute(IJobExecutionContext context)
    {
        var normalWateringPlants = await _reminderLogicService.FetchUserPlantsWithMediumWateringNeedsAsync();
    
        foreach (var plantName in normalWateringPlants)
        {
            _notificationService.ShowWateringNotification(plantName);
        }
    }
}