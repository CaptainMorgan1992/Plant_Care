namespace Auth0_Blazor.Models;

public class User
{
    public int Id { get; set; }
    public string OwnerId { get; set; } // This is the Auth0 user ID
    public string Name { get; set; }
    public List<UserPlant> Plants { get; set; }
}