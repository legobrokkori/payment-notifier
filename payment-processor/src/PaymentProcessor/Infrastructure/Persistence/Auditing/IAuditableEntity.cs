// src/PaymentProcessor/Infrastructure/Persistence/Auditing/IAuditableEntity.cs

namespace PaymentProcessor.Infrastructure.Persistence.Auditing
{
    /// <summary>
    /// Provides auditing fields for entity tracking.
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the timestamp when the entity was created (UTC).
        /// </summary>
        DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the entity was last updated (UTC).
        /// </summary>
        DateTimeOffset UpdatedAt { get; set; }
    }
}
