using System.ComponentModel.DataAnnotations;

namespace Auth0_Blazor.Models;

public class User
{
    public int Id { get; init; }
    
    [MaxLength(100)]
    public required string OwnerId { get; set; } // This is the Auth0 user ID
    
    [MaxLength(70)]
    public string? Name { get; set; }
    
    public List<UserPlant>? Plants { get; set; }
    
    public bool IsAdmin { get; set; } = false;
}