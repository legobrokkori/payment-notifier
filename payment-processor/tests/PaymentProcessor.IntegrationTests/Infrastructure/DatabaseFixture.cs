using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PaymentProcessor.Infrastructure.Persistence;

namespace PaymentProcessor.IntegrationTests.Infrastructure;

/// <summary>
/// Database fixture that creates the test database once for all tests.
/// Each test should use transactions to ensure isolation.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        // Create service provider once
        ServiceProvider = TestAppFactory.Create();

        // Create database schema once
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure clean slate - delete only if exists
        if (await dbContext.Database.CanConnectAsync())
        {
            await dbContext.Database.EnsureDeletedAsync();
        }

        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        // Leave test database for inspection and reuse
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new scope for test isolation.
    /// Each test should use this to get a fresh DbContext with a transaction.
    /// </summary>
    public IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }
}
