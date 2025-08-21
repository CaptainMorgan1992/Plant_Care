using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Services;

public class UserService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<UserService> _logger;
    private readonly ApplicationDbContext _db;

    public UserService (
        AuthenticationStateProvider authStateProvider,
        ILogger<UserService> logger,
        ApplicationDbContext db)
    {
        _authStateProvider = authStateProvider;
        _logger = logger;
        _db = db;
    }


    public async Task<string> GetUserAuth0IdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("UserId is missing. Throwing exception.");
            throw new InvalidOperationException("UserId could not be found for the current user.");
        }
        
        return userId;
    }
    
    public async Task<int?> GetUserIdByOwnerIdAsync(string ownerId)
    {
        var user = await _db.Users
            .Where(u => u.OwnerId == ownerId)
            .FirstOrDefaultAsync();

        return user?.Id;
    }

    public async Task<string> FetchCurrentUserAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userName = user.Identity?.Name;
        
        if (string.IsNullOrWhiteSpace(userName) || userName.Length < 2)
        {
            return "Unknown";
        }

        return userName;
    }

    public async Task SaveUserOnClick()
    {
        var userId = await GetUserAuth0IdAsync();
        var username = await FetchCurrentUserAsync();

        var exists = await _db.Users.AnyAsync(u => u.OwnerId == userId);

        if (!exists)
        {
            await SaveUserDetailsToDb(userId, username);
            _logger.LogInformation("User {Username} was saved.", username);
        }

    }
    
    private async Task SaveUserDetailsToDb(string userId, string username)
    {
        var newUser = new User
        {
            OwnerId = userId,
            Name = username
        };
        
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
    }
}