using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services.IService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Services;

public class UserService : IUserService
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

    public async Task<string?> GetUserAuth0IdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        userId = DoesUserIdHaveValue(userId);
        
        return userId;
    }
    
    public string? DoesUserIdHaveValue(string? userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return userId;
        }

        _logger.LogWarning("No userId found. User details will not be saved.");
        return null;
    }
    
    public async Task<bool> IsUserAdminAsync(string ownerId)
    {
        var user = await _db.Users
            .Where(u => u.OwnerId == ownerId)
            .FirstOrDefaultAsync();

        return user?.IsAdmin ?? false;
    }
    
    public async Task<int?> GetUserIdByOwnerIdAsync(string ownerId)
    {
        var user = await _db.Users
            .Where(u => u.OwnerId == ownerId)
            .FirstOrDefaultAsync();

        return user?.Id;
    }

    public async Task<string> FetchCurrentUserNameAsync()
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
        var username = await FetchCurrentUserNameAsync();
        var validUserId = IsUserIdNullOrWhiteSpace(userId);
        if (validUserId)
        {
            if (!await DoesUserExist(userId!))
            {
                await SaveUserDetailsToDb(userId!, username);
                _logger.LogInformation("User {Username} was saved.", username);
            }
        }
    }
    
    public async Task<bool> DoesUserExist(string userId)
    {
        return await _db.Users.AnyAsync(u => u.OwnerId == userId);
    }
    
    public bool IsUserIdNullOrWhiteSpace(string? userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return true;
        }

        _logger.LogWarning("No userId found. User details will not be saved.");
        return false;
    }
    
    public async Task SaveUserDetailsToDb(string userId, string username)
    {
        var newUser = new User
        {
            OwnerId = userId,
            Name = username
        };
        
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
    }
    
    /* This next */
    public void ValidateOwnerId(string? ownerId)
    {
        if (!string.IsNullOrWhiteSpace(ownerId))
        {
            return;
        }

        _logger.LogError("OwnerId is null or empty.");
        throw new ArgumentNullException(nameof(ownerId), "OwnerId cannot be null or empty.");
    }
    
    public void DoesUserIdHaveIntValue(int? userId)
    {
        if (!userId.HasValue)
        {
            throw new ArgumentNullException(nameof(userId), "UserId cannot be null.");
        }
    }

}