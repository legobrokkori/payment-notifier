namespace PaymentProcessor.Infrastructure.Persistence.Entities.Inbox
{
    using PaymentProcessor.Infrastructure.Persistence.Auditing;

    /// <summary>
    /// Represents an event received from an external system and tracked for idempotent processing.
    /// </summary>
    public class InboxEvent : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique event identifier. Used as the primary key.
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the raw JSON payload of the event.
        /// </summary>
        public string RawPayload { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processing status of the event (Pending, Processing, Processed, Failed).
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Gets or sets how many times processing was attempted.
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the timestamp when processing started (nullable).
        /// </summary>
        public DateTimeOffset? ProcessingStartedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the event was received.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// Useful for tracking reprocessing or updates.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
