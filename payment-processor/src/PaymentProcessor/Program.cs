using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Interfaces;
using PaymentProcessor.Infrastructure.Configurations;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.Infrastructure.Redis;

class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear(); // Optional: reset to custom config only
                config
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Config classes
                services.Configure<RedisSettings>(configuration.GetSection("Redis"));
                services.Configure<DatabaseSettings>(configuration.GetSection("Database"));

                // Logging
                services.AddLogging(logging => logging.AddConsole());

                // Redis & Repository
                services.AddSingleton<IRedisConsumer, RedisConsumer>();
                services.AddScoped<IPaymentRepository, PaymentRepository>();

                // EF DbContext
                var dbSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(dbSettings.ConnectionString));

                // Worker
                services.AddScoped<PaymentWorker>();
            })
            .Build();

        // Resolve and run the worker
        using var scope = host.Services.CreateScope();
        var worker = scope.ServiceProvider.GetRequiredService<PaymentWorker>();

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await worker.RunWorkerAsync(cts.Token);
    }
}