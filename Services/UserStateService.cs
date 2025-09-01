using Auth0_Blazor.Records;

namespace Auth0_Blazor.Services;

public class UserStateService
{
    private string _ownerId;

    public void SetOwnerId(string ownerId)
    {
        _ownerId = ownerId;
    }

    public string GetOwnerId() => _ownerId;
}