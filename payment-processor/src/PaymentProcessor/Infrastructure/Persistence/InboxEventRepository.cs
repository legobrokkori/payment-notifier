// <copyright file="InboxEventRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;

    using PaymentProcessor.Domain.Events;
    using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;

    /// <summary>
    /// Provides operations to insert and update <see cref="InboxEvent"/> records in the database.
    /// </summary>
    public class InboxEventRepository
    {
        private readonly AppDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboxEventRepository"/> class.
        /// </summary>
        /// <param name="db">The application's database context.</param>
        public InboxEventRepository(AppDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Inserts a new inbox event into the database.
        /// </summary>
        /// <param name="eventId">The event ID (idempotency key).</param>
        /// <param name="rawPayload">The raw event payload as JSON or serialized format.</param>
        /// <param name="cancellationToken">The cancellation token for the async operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InsertAsync(string eventId, string rawPayload, CancellationToken cancellationToken)
        {
            var entity = new InboxEvent
            {
                EventId = eventId,
                RawPayload = rawPayload,
                Status = InboxEventStatus.Pending.ToString(),
            };

            this.db.InboxEvents.Add(entity);
            await this.db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates the status of an existing inbox event.
        /// </summary>
        /// <param name="eventId">The event ID to update.</param>
        /// <param name="newStatus">The new status to set.</param>
        /// <param name="cancellationToken">The cancellation token for the async operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateStatusAsync(string eventId, InboxEventStatus newStatus, CancellationToken cancellationToken)
        {
            var entity = await this.db.InboxEvents
                .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);

            if (entity is null)
            {
                throw new InvalidOperationException($"InboxEvent with EventId '{eventId}' not found.");
            }

            entity.Status = newStatus.ToString();
            await this.db.SaveChangesAsync(cancellationToken);
        }
    }
}
