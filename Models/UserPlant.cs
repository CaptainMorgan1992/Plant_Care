namespace Auth0_Blazor.Models;

public class UserPlant
{
    public int Id { get; init; }
    public required int PlantId { get; init; }
    public required int UserId { get; init; }
}
