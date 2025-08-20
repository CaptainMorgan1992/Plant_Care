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

        // Hämta Auth0 användar-id (sub claim)
        var userId = user.FindFirst("sub")?.Value;
        _logger.LogInformation("UserId (sub): {UserId}", userId);

        // (Valfritt) Logga ut alla claims
        foreach (var claim in user.Claims)
        {
            _logger.LogInformation("Claim type: {Type}, value: {Value}", claim.Type, claim.Value);
        }

        return userId;
    }
}