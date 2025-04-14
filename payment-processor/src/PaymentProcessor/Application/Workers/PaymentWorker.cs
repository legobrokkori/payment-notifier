using System;
using System.Threading;
using System.Threading.Tasks;
using PaymentProcessor.Domain.Interfaces;

namespace PaymentProcessor.Application.Workers
{
    public class PaymentWorker
    {
        private readonly IRedisConsumer _consumer;
        private readonly IPaymentRepository _repository;

        public PaymentWorker(IRedisConsumer consumer, IPaymentRepository repository)
        {
            _consumer = consumer;
            _repository = repository;
        }

        // RunWorkerAsync: Processes a limited number of messages from Redis.
        // Designed for short-lived execution to stay within Render's free tier limits.
        public async Task RunWorkerAsync(CancellationToken cancellationToken)
        {

            Console.WriteLine("Worker started.");

            int maxMessages = 100; // TODO: Move this to configuration (e.g., environment variable)
            int processed = 0;

            while (!cancellationToken.IsCancellationRequested && processed < maxMessages)
            {
                var paymentEvent = await _consumer.DequeueAsync(cancellationToken);

                // Stop processing if no more messages are available in the queue.
                // This prevents the loop from running unnecessarily, helping Render auto-sleep the service.
                if (paymentEvent == null) break;

                Console.WriteLine($"Processing event: {paymentEvent.Id}");
                await _repository.SaveAsync(paymentEvent, cancellationToken);
                processed++;
            }
        }
    }
}
