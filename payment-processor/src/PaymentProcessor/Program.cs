// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PaymentProcessor.Application.Services;
using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Repositories;
using PaymentProcessor.Domain.Interfaces;
using PaymentProcessor.Infrastructure.Configurations;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.Infrastructure.Persistence.Repositories;
using PaymentProcessor.Infrastructure.Redis;
using StackExchange.Redis;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();
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

                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
                    return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
                });

                // Redis & EF dependencies
                services.AddSingleton<IPaymentEventSource, RedisPaymentEventSource>();
                services.AddScoped<IPaymentRepository, PaymentRepository>();
                services.AddScoped<IInboxEventRepository, InboxEventRepository>();
                services.AddScoped<IInboxEventLogRepository, InboxEventLogRepository>();
                services.AddScoped<IInboxEventProcessor, InboxEventProcessor>();
                services.AddScoped<RedisInboxIngestWorker>();
                services.AddScoped<InboxToPaymentWorker>();

                // EF DbContext
                var dbSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
                if (dbSettings == null)
                {
                    throw new InvalidOperationException("Database settings not found.");
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(dbSettings.ConnectionString));
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var mode = args.FirstOrDefault();
        switch (mode)
        {
            case "redis":
                var redisWorker = services.GetRequiredService<RedisInboxIngestWorker>();
                await redisWorker.RunAsync(CancellationToken.None);
                break;

            case "inbox":
                var inboxWorker = services.GetRequiredService<InboxToPaymentWorker>();
                await inboxWorker.ProcessPendingEventsAsync(CancellationToken.None);
                break;

            default:
                Console.WriteLine("Usage: dotnet run -- redis | inbox");
                break;
        }
    }
}
