using Microsoft.AspNetCore.Components.Authorization;
namespace Auth0_Blazor.Services;

public class UserService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<UserService> _logger;

    public UserService (AuthenticationStateProvider authStateProvider, ILogger<UserService> logger)
    {
        _authStateProvider = authStateProvider;
        _logger = logger;
    }


    public async Task<string?> GetUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("UserId (nameidentifier): {UserId}", userId);
        return userId;
        
    }
}