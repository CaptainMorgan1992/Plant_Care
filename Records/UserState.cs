namespace Auth0_Blazor.Records;

/*
 * Records are immutable data carriers. That is why we use them for UserState.
 * The UserState record holds the OwnerId of the currently logged-in user.
 */
public record UserState(string OwnerId);