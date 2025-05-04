// <copyright file="InboxEventRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;

    using PaymentProcessor.Domain.Events;
    using PaymentProcessor.Domain.Interfaces;
    using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;

    /// <summary>
    /// Repository responsible for handling operations on InboxEvent entities.
    /// </summary>
    public class InboxEventRepository : IInboxEventRepository
    {
        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboxEventRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        public InboxEventRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
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

                // inbox.FailureReason = reason;
                await this.dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
