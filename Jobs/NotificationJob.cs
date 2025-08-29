using Auth0_Blazor.Services;
using Quartz;

namespace Auth0_Blazor.Jobs;

/*
 * This class handles what the job is doing when it is triggered.
 */
public class NotificationJob : IJob
{
    private readonly NotificationService _notificationService;
    
    public NotificationJob (NotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    
    public Task Execute(IJobExecutionContext context)
    {
        _notificationService.ShowNotificiation("This is a scheduled notification every 10th second.");
        return Task.CompletedTask;
    }
}