using Auth0_Blazor.Services.IService;

namespace Auth0_Blazor.Services;

public class UtilityService : IUtilityService
{
    public string? TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Length > maxLength ? string.Concat(text.AsSpan(0, maxLength), "...") : text;
    }
}