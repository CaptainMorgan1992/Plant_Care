namespace Auth0_Blazor.Services.IService;

public interface IUserService
{
    Task<string?> GetUserAuth0IdAsync();
    Task<bool> IsUserAdminAsync(string ownerId);
    Task<int?> GetUserIdByOwnerIdAsync(string ownerId);
    Task<string> FetchCurrentUserAsync();
    Task SaveUserOnClick();
    Task SaveUserDetailsToDb(string userId, string userName);
    void DoesUserIdHaveValue(int? userId);
    bool IsUserIdNullOrWhiteSpace(string? userId);


}