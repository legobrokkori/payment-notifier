// <copyright file="InboxEventRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using PaymentProcessor.Domain.Events;
    using PaymentProcessor.Domain.Interfaces;
    using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;

    /// <summary>
    /// Repository responsible for handling operations on InboxEvent entities.
    /// </summary>
    public class InboxEventRepository : IInboxEventRepository
    {
        private readonly AppDbContext dbContext;
        private readonly ILogger<InboxEventRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboxEventRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        /// <param name="logger">Logger instance for this repository.</param>
        public InboxEventRepository(AppDbContext dbContext, ILogger<InboxEventRepository> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task SaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken)
        {
            this.dbContext.InboxEvents.Add(inboxEvent);
            await this.dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InboxEvent>> GetPendingEventsAsync(CancellationToken cancellationToken)
        {
            return await this.dbContext.InboxEvents
                .Where(e => e.Status == InboxEventStatus.Pending.ToString())
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task MarkAsProcessedAsync(long id, CancellationToken cancellationToken)
        {
            var inbox = await this.dbContext.InboxEvents.FindAsync(new object[] { id }, cancellationToken);
            if (inbox != null)
            {
                inbox.Status = InboxEventStatus.Completed.ToString();
                await this.dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task MarkAsFailedAsync(long id, string reason, CancellationToken cancellationToken)
        {
            var inbox = await this.dbContext.InboxEvents.FindAsync(new object[] { id }, cancellationToken);
            if (inbox != null)
            {
                inbox.Status = InboxEventStatus.Failed.ToString();

                // Uncomment if failure reason column exists
                await this.dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Tries to insert a new inbox event. Skips if the event already exists or if a PK violation occurs.
        /// </summary>
        /// <param name="inboxEvent">The inbox event entity to be saved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>representing the asynchronous operation.</returns>
        public async Task TrySaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken)
        {
            if (await this.ExistsAsync(inboxEvent.EventId, cancellationToken))
            {
                this.logger.LogWarning("InboxEvent with EventId={EventId} already exists. Skipping.", inboxEvent.EventId);
                return;
            }

            try
            {
                this.dbContext.InboxEvents.Add(inboxEvent);
                await this.dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsPrimaryKeyViolation(ex))
            {
                this.logger.LogWarning(ex, "Duplicate key detected for EventId={EventId}. Skipping insert.", inboxEvent.EventId);
            }
        }

        /// <summary>
        /// Detects if a DbUpdateException is due to a primary key constraint violation.
        /// Implementation may vary depending on the database provider.
        /// </summary>
        private static bool IsPrimaryKeyViolation(DbUpdateException ex)
        {
            // PostgreSQL example
            return ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true;
        }

        /// <summary>
        /// Checks if an inbox event with the given ID already exists.
        /// </summary>
        private async Task<bool> ExistsAsync(string eventId, CancellationToken cancellationToken)
        {
            return await this.dbContext.InboxEvents
                .AsNoTracking()
                .AnyAsync(e => e.EventId == eventId, cancellationToken);
        }
    }
}
