using Auth0_Blazor.Services;
using Quartz;

namespace Auth0_Blazor.Jobs;

public class WateringNotificationJob : IJob
{
    
    private readonly NotificationService _notificationService;
    private readonly ReminderLogicService _reminderLogicService;
    private readonly ILogger<WateringNotificationJob> _logger;
    
    public WateringNotificationJob (
        NotificationService notificationService,
        ReminderLogicService reminderLogicService,
        ILogger<WateringNotificationJob> logger)
    {
        _notificationService = notificationService;
        _reminderLogicService = reminderLogicService;
        _logger = logger;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var normalWateringPlants = await _reminderLogicService.FetchUserPlantsWithMediumWateringNeedsAsync();
    
        foreach (var plantName in normalWateringPlants)
        {
            _notificationService.ShowWateringNotification(plantName);
        }
    }
}