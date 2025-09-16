using Auth0_Blazor.Records;
using Auth0_Blazor.Services.IService;

namespace Auth0_Blazor.Services;

public class UserStateService : IUserStateService
{
    
    ILogger<UserStateService> _logger;
    
    public UserStateService(ILogger<UserStateService> logger)
    {
        _logger = logger;
    }
    public string? OwnerId { get; private set; }

    public void SetOwnerId(string ownerId)
    {
        OwnerId = ownerId;
    }
}