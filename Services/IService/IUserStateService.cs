namespace Auth0_Blazor.Services.IService;

public interface IUserStateService
{
    string? OwnerId { get; }
    void SetOwnerId(string ownerId);
}