namespace Auth0_Blazor.Services;
/*
 * This class serves as a channel for sending notifications from background jobs to the UI.
 * No other logic.
 */
public class NotificationService
{
    public event Action<string>? OnNotify;
    public event Action<string>? OnWateringNotify;
    
    public void ShowNotificiation(string message)
    {
        OnNotify?.Invoke(message);
    }
    
    public void ShowWateringNotification(string plantName)
    {
        OnWateringNotify?.Invoke(plantName);
    }
}