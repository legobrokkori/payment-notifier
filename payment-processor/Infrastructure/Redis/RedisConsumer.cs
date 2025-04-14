using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentProcessor.Domain.Interfaces;

namespace PaymentProcessor.Infrastructure.Redis
{
    public class RedisConsumer : IRedisConsumer
    {
        private readonly ILogger<RedisConsumer> _logger;

        public RedisConsumer(ILogger<RedisConsumer> logger)
        {
            _logger = logger;
        }

        public Task ConsumeAsync(CancellationToken cancellationToken)
        {
            // Dummy logic for now
            _logger.LogInformation("ConsumeAsync called. [Stub Implementation]");
            return Task.CompletedTask;
        }
    }
}
