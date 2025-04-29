namespace PaymentProcessor.Application.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using PaymentProcessor.Domain.Events;
    using PaymentProcessor.Infrastructure.Persistence;
    using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;

    /// <summary>
    /// Processes pending inbox events by converting them to domain events and handling them.
    /// </summary>
    public class InboxEventProcessor
    {
        private readonly AppDbContext db;
        private readonly ILogger<InboxEventProcessor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboxEventProcessor"/> class.
        /// </summary>
        /// <param name="db">The application's database context.</param>
        /// <param name="logger">The logger instance.</param>
        public InboxEventProcessor(AppDbContext db, ILogger<InboxEventProcessor> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// Processes all pending inbox events.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
        {
            var pendingEvents = await this.db.InboxEvents
                .Where(e => e.Status == InboxEventStatus.Pending.ToString())
                .ToListAsync(cancellationToken);

            foreach (var inboxEvent in pendingEvents)
            {
                try
                {
                    // Simulate processing the event
                    this.logger.LogInformation("Processing event: {EventId}", inboxEvent.EventId);

                    // TODO: Deserialize inboxEvent.RawPayload into a domain event and handle it
                    inboxEvent.Status = InboxEventStatus.Completed.ToString();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to process event: {EventId}", inboxEvent.EventId);
                    inboxEvent.Status = InboxEventStatus.Failed.ToString();
                }
            }

            await this.db.SaveChangesAsync(cancellationToken);
        }
    }
}
