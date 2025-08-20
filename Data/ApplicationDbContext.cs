using Auth0_Blazor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Plant> Plants { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<UserPlant> UserPlants { get; set; }
}