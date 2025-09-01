using Fluxor;
namespace Auth0_Blazor.FluxorState;

public class UserReducers
{
    [ReducerMethod]
    public static UserState ReduceSetOwnerIdAction(UserState state, SetOwnerIdAction action) =>
        state with { OwnerId = action.OwnerId };
}