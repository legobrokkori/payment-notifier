using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Interfaces;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.Infrastructure.Redis;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<IRedisConsumer, RedisConsumer>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddHostedService<PaymentWorker>();
    })
    .Build();

await host.RunAsync();