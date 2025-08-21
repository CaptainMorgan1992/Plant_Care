using System.ComponentModel.DataAnnotations.Schema;
namespace Auth0_Blazor.Models;

public class UserPlant
{
    public int Id { get; init; }
    public required int PlantId { get; init; }
    public required int UserId { get; init; }
    
    // Navigation property till Plant
    [ForeignKey(nameof(PlantId))]
    public Plant Plant { get; set; }

    // Navigation property till User 
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}
