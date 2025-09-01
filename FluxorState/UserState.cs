using Fluxor;

namespace Auth0_Blazor.FluxorState;

[FeatureState]
public record UserState
{
    public string OwnerId { get; init; } 
    
    // Parameterless constructor required by Fluxor
    public UserState() : this(string.Empty) { }

    public UserState(string ownerId)
    {
        OwnerId = ownerId;
    }
};