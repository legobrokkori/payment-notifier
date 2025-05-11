namespace PaymentProcessor.Domain.Interfaces
{
    using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;

    /// <summary>
    /// Repository interface for accessing and updating InboxEvent records.
    /// Supports the Inbox pattern for reliable message processing.
    /// </summary>
    public interface IInboxEventRepository
    {
        /// <summary>
        /// Saves a new inbox event received from an external queue.
        /// </summary>
        /// <param name="inboxEvent">The inbox event entity to be persisted.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Saves an inbox event only if it does not already exist.
        /// Ensures idempotency by preventing duplicate inserts.
        /// </summary>
        /// <param name="inboxEvent">The inbox event entity to be persisted.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task TrySaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Returns all inbox events that are pending processing.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A list of pending inbox events.</returns>
        Task<IReadOnlyList<InboxEvent>> GetPendingEventsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Marks the specified inbox event as successfully processed.
        /// </summary>
        /// <param name="id">The ID of the inbox event.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MarkAsProcessedAsync(long id, CancellationToken cancellationToken);

        /// <summary>
        /// Marks the specified inbox event as failed and stores the error reason.
        /// </summary>
        /// <param name="id">The ID of the inbox event.</param>
        /// <param name="reason">Reason for the failure (e.g., exception message).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MarkAsFailedAsync(long id, string reason, CancellationToken cancellationToken);
    }
}
