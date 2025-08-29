using Fluxor;
namespace Auth0_Blazor.FluxorState;

public class UserReducers
{
    [ReducerMethod]
    public static UserState ReduceSetUserIdAction(UserState state, SetUserIdAction action) =>
        state with { UserId = action.UserId };
}