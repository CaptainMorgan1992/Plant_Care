namespace Auth0_Blazor.FluxorState;
using Fluxor;

public class UserFeature : Feature <UserState>
{
    public override string GetName() => "User";
    protected override UserState GetInitialState() => new(OwnerId: string.Empty);
}