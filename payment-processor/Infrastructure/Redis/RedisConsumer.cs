using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Interfaces;
using PaymentProcessor.Infrastructure.Configurations;

namespace PaymentProcessor.Infrastructure.Redis
{
public class RedisConsumer : IRedisConsumer
{
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisConsumer> _logger;

    public RedisConsumer(IOptions<RedisSettings> options, ILogger<RedisConsumer> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Task<PaymentEvent?> DequeueAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Simulating Redis dequeue from queue: {Queue}", _settings.Queue);
        return Task.FromResult<PaymentEvent?>(null);
    }
}
}
