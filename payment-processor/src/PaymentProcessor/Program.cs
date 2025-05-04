// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PaymentProcessor.Application.Services;
using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Interfaces;
using PaymentProcessor.Infrastructure.BackgroundServices;
using PaymentProcessor.Infrastructure.Configurations;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.Infrastructure.Redis;

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

                // Redis & EF dependencies
                services.AddSingleton<IPaymentEventSource, RedisPaymentEventSource>();
                services.AddScoped<IPaymentRepository, PaymentRepository>();
                services.AddScoped<IInboxEventRepository, InboxEventRepository>();
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

                // Register background services
                services.AddHostedService<RedisInboxIngestBackgroundService>();
                services.AddHostedService<InboxToPaymentBackgroundService>();
            })
            .Build();

        await host.RunAsync();
    }
}
