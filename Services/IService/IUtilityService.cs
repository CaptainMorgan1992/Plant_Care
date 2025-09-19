namespace Auth0_Blazor.Services.IService;

public interface IUtilityService
{
    string? TruncateText(string? text, int maxLength);
}