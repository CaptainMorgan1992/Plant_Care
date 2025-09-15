namespace Auth0_Blazor.Services;

public class UtilityService
{
    public string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
    }
}