namespace Auth0_Blazor.Services.IService;

public interface IUserService
{
    Task<string?> GetUserAuth0IdAsync();
    Task<bool> IsUserAdminAsync(string ownerId);
    Task<int?> GetUserIdByOwnerIdAsync(string ownerId);
    Task<string> FetchCurrentUserNameAsync();
    Task SaveUserOnClick();
    Task SaveUserDetailsToDb(string userId, string userName);
    void DoesUserIdHaveIntValue(int? userId);
    string? DoesUserIdHaveValue(string? userId);
    bool IsUserIdNullOrWhiteSpace(string? userId);


}