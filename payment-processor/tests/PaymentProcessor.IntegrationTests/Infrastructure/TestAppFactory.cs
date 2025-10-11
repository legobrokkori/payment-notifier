using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentProcessor.Application.Services;
using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Repositories;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.Infrastructure.Persistence.Repositories;

namespace PaymentProcessor.IntegrationTests.Infrastructure
{
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

            // DbContext
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            // Logging
            services.AddLogging(builder => builder.AddConsole());

            // Repositories
            services.AddScoped<IInboxEventRepository, InboxEventRepository>();
            services.AddScoped<IInboxEventLogRepository, InboxEventLogRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            // Services / Workers
            services.AddScoped<IInboxEventProcessor, InboxEventProcessor>();
            services.AddScoped<InboxToPaymentWorker>();

            return services.BuildServiceProvider();
        }
    }
}
