namespace Auth0_Blazor.Services;

public class NotificationService
{
    public event Action<string>? OnNotify;
    
    public void ShowNotificiation(string message)
    {
        OnNotify?.Invoke(message);
    }
}