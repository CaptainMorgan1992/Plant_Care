using Auth0_Blazor.Models;

namespace Auth0_Blazor.Services.IService;

public interface INotificationService
{
    void ShowWateringNotification(User user, string plantName);
}