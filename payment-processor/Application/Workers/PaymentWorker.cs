// Application/Workers/PaymentWorker.cs
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentProcessor.Domain.Interfaces;

namespace PaymentProcessor.Application.Workers
{
    public class PaymentWorker
    {
        private readonly IRedisConsumer _consumer;
        private readonly ILogger<PaymentWorker> _logger;

        public PaymentWorker(IRedisConsumer consumer, ILogger<PaymentWorker> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting payment worker...");
            await _consumer.ConsumeAsync(cancellationToken);
        }
    }
}
