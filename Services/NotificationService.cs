using Auth0_Blazor.Enums;
using Auth0_Blazor.Models;

namespace Auth0_Blazor.Services;
/*
 * This class serves as a channel for sending notifications from background jobs to the UI.
 * No other logic.
 */
public class NotificationService
{
    public event Action<string, string>? OnWateringNotify;
    
    public void ShowWateringNotification(User user, string plantName)
    {
        OnWateringNotify?.Invoke(plantName, user.OwnerId);
    }
}