using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Auth0_Blazor.Data;

public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
    {
        _options = options;
    }

    public ApplicationDbContext CreateDbContext() => new ApplicationDbContext(_options);

    public ValueTask<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => new ValueTask<ApplicationDbContext>(new ApplicationDbContext(_options));
}
