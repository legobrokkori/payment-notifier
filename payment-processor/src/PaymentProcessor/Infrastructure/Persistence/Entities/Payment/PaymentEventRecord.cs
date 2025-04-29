// <copyright file="PaymentEventRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Persistence.Entities.Payment
{
    using System;

    using PaymentProcessor.Infrastructure.Persistence.Auditing;

    /// <summary>
    /// Represents the database entity for storing payment events.
    /// </summary>
    public class PaymentEventRecord : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier of the payment event.
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the payment currency.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the event occurred (UTC).
        /// </summary>
        public DateTimeOffset EventAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was created in the database.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// Useful for tracking reprocessing or updates.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
