// <copyright file="RedisInboxIngestWorker.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Application.Workers
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using PaymentProcessor.Domain.Entities;
    using PaymentProcessor.Domain.Events;
    using PaymentProcessor.Domain.Interfaces;
    using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;

    /// <summary>
    /// Background worker that consumes payment events from Redis
    /// and saves them to the Inbox table for further processing.
    /// </summary>
    public class RedisInboxIngestWorker
    {
        private readonly IPaymentEventSource eventSource;
        private readonly IInboxEventRepository inboxRepository;
        private readonly ILogger<RedisInboxIngestWorker> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisInboxIngestWorker"/> class.
        /// </summary>
        /// <param name="eventSource">The Redis queue consumer.</param>
        /// <param name="inboxRepository">The inbox repository for persisting events.</param>
        /// <param name="logger">The logger instance.</param>
        public RedisInboxIngestWorker(
            IPaymentEventSource eventSource,
            IInboxEventRepository inboxRepository,
            ILogger<RedisInboxIngestWorker> logger)
        {
            this.eventSource = eventSource;
            this.inboxRepository = inboxRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Continuously pulls events from Redis and stores them in the InboxEvent table.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the loop gracefully.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("RedisInboxIngestWorker started.");

            int maxMessages = 100; // Can be configurable via env
            int processed = 0;

            while (!cancellationToken.IsCancellationRequested && processed < maxMessages)
            {
                var paymentEvent = await this.eventSource.DequeueAsync(cancellationToken);

                if (paymentEvent == null)
                {
                    break; // No message = safe to stop
                }

                var payload = JsonSerializer.Serialize(paymentEvent);

                var inboxEvent = new InboxEvent
                {
                    EventId = paymentEvent.Id,
                    RawPayload = payload,
                    Status = InboxEventStatus.Pending.ToString(),
                };

                try
                {
                    await this.inboxRepository.TrySaveAsync(inboxEvent, cancellationToken);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Duplicate EventId"))
                {
                    this.logger.LogWarning("Skipping duplicate event. EventId={EventId}", paymentEvent.Id);
                    continue;
                }

                this.logger.LogInformation("Enqueued InboxEvent with EventId={EventId}", paymentEvent.Id);
                processed++;
            }

            this.logger.LogInformation("RedisInboxIngestWorker completed. Total processed: {Count}", processed);
        }
    }
}
