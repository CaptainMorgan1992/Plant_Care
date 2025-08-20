using Auth0_Blazor.Models;
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
        fetchCurrentUser();
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("UserId (nameidentifier): {UserId}", userId);
        _logger.LogInformation("UserName: {UserName}", user.Identity?.Name);
        return userId;
        
    }

    public async Task<User> fetchCurrentUser()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var claimsUser = authState.User;

        var user = new User
        {
            OwnerId = claimsUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Name = claimsUser.Identity?.Name,
            // Lägg till fler properties om du har!
        };

        return user;
    }
}