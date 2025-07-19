// <copyright file="InboxToPaymentWorker.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Application.Workers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using PaymentProcessor.Domain.Entities;
    using PaymentProcessor.Domain.Events;
    using PaymentProcessor.Infrastructure.Persistence;

    /// <summary>
    /// Processes pending inbox events and updates their status after processing.
    /// </summary>
    public class InboxToPaymentWorker
    {
        private readonly AppDbContext dbContext;
        private readonly ILogger<InboxToPaymentWorker> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboxToPaymentWorker"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        /// <param name="logger">The logger instance.</param>
        public InboxToPaymentWorker(AppDbContext dbContext, ILogger<InboxToPaymentWorker> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        /// <summary>
        /// Processes all pending inbox events.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the async operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
        {
            var pendingEvents = await this.dbContext.InboxEvents
                .Where(e => e.Status == InboxEventStatus.Pending)
                .ToListAsync(cancellationToken);

            foreach (var inboxEvent in pendingEvents)
            {
                try
                {
                    this.logger.LogInformation("Processing InboxEvent with EventId={EventId}", inboxEvent.EventId);

                    // Attempt to parse the raw payload into a domain PaymentEvent
                    var paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(inboxEvent.RawPayload);

                    if (paymentEvent == null)
                    {
                        this.logger.LogWarning("Failed to deserialize PaymentEvent. Marking as Failed. EventId={EventId}", inboxEvent.EventId);
                        inboxEvent.MarkAsFailed();
                        continue;
                    }

                    // TODO: Actually process the paymentEvent (e.g., business logic here)
                    inboxEvent.MarkAsCompleted();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error processing InboxEvent. Marking as Failed. EventId={EventId}", inboxEvent.EventId);
                    inboxEvent.MarkAsFailed();
                }
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
