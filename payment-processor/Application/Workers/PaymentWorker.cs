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

        // RunWorkerAsync: Processes messages from Redis in a loop
        public async Task RunWorkerAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Worker started.");
            while (!cancellationToken.IsCancellationRequested)
            {
                var paymentEvent = await _consumer.DequeueAsync(cancellationToken);
                if (paymentEvent == null)
                {
                    await Task.Delay(100, cancellationToken); // backoff
                    continue;
                }

                Console.WriteLine($"Processing event: {paymentEvent.Id}");

                // TODO: validation, transformation etc.

                await _repository.SaveAsync(paymentEvent, cancellationToken);
            }
        }
    }
}
