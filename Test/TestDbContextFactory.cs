using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Auth0_Blazor.Data;

public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly ApplicationDbContext _dbContext;

    public TestDbContextFactory(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ApplicationDbContext CreateDbContext() => _dbContext;

    public ValueTask<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => new ValueTask<ApplicationDbContext>(_dbContext);
}