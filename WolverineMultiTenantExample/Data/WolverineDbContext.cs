using Microsoft.EntityFrameworkCore;

namespace WolverineMultiTenantExample.Data;

public class WolverineDbContext : DbContext
{
    public WolverineDbContext(DbContextOptions<WolverineDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Wolverine's envelope storage
        // This will be used by Wolverine for persistent messaging
        base.OnModelCreating(modelBuilder);
    }
}

public static class DatabaseExtensions
{
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WolverineDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}