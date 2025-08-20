using System.ComponentModel.DataAnnotations;

namespace Auth0_Blazor.Models;

public class Plant
{
    public int Id { get; init; }
    
    [MaxLength(70)]
    public required string Name { get; set; }
    
    [MaxLength(250)]
    public required string Description { get; set; }
    
    [MaxLength(2000)]
    public required string ImageUrl { get; set; }
    
    [MaxLength(50)]
    public string? Origin { get; set; }
}