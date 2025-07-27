using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentProcessor.Infrastructure.Persistence;

namespace PaymentProcessor.IntegrationTests.Infrastructure;

/// <summary>
/// Factory to create a test service provider with real DB context.
/// </summary>
public static class TestAppFactory
{
    public static IServiceProvider Create()
    {
        var services = new ServiceCollection();

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing DB_CONNECTION_STRING environment variable.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddLogging(builder => builder.AddConsole());

        return services.BuildServiceProvider();
    }
}
