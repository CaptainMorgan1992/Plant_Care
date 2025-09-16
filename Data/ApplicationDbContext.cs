using Auth0_Blazor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth0_Blazor.Data;

/*
 A class which represents the DB in EF Core. It inherits from DbContext, which is the
 base for every database interaction with Entity Framework Core.
 */
public class ApplicationDbContext : DbContext
{
    /*
     Constructor that recieves 'options' - connection string, DB provider,
     and passes them on to the DbContext 
     */
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSet properties represent the tables in the database
    public DbSet<Plant> Plants { get; set; }
    
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<UserPlant> UserPlants { get; set; }
}