// <copyright file="PaymentWorker.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Application.Workers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using PaymentProcessor.Domain.Interfaces;

    /// <summary>
    /// Background worker that dequeues payment events from Redis and saves them to the database.
    /// </summary>
    public class PaymentWorker
    {
        private readonly IPaymentEventSource consumer;
        private readonly IPaymentRepository repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentWorker"/> class.
        /// </summary>
        /// <param name="consumer">The Redis consumer responsible for dequeuing payment events.</param>
        /// <param name="repository">The repository for persisting payment events to the database.</param>
        public PaymentWorker(IPaymentEventSource consumer, IPaymentRepository repository)
        {
            this.consumer = consumer;
            this.repository = repository;
        }

        /// <summary>
        /// Starts the worker loop, fetching and processing events from Redis until max limit or cancellation.
        /// </summary>
        /// <param name="cancellationToken">Token used to signal cancellation of the worker loop.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RunWorkerAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Worker started.");

            int maxMessages = 100; // TODO: Move this to configuration (e.g., environment variable)
            int processed = 0;

            while (!cancellationToken.IsCancellationRequested && processed < maxMessages)
            {
                var paymentEvent = await this.consumer.DequeueAsync(cancellationToken);

                // Stop processing if no more messages are available in the queue.
                // This prevents the loop from running unnecessarily, helping Render auto-sleep the service.
                if (paymentEvent == null)
                {
                    break;
                }

                Console.WriteLine($"Processing event: {paymentEvent.Id}");
                await this.repository.SaveAsync(paymentEvent, cancellationToken);
                processed++;
            }
        }
    }
}
