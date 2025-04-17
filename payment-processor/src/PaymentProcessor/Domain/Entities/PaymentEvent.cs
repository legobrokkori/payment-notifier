// <copyright file="PaymentEvent.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Domain.Entities
{
    using System;

    /// <summary>
    /// Domain entity representing a payment event received via webhook.
    /// </summary>
    public class PaymentEvent
    {
        /// <summary>
        /// Valid status values for a payment event.
        /// </summary>
        private static readonly string[] ValidStatuses = { "paid", "failed", "cancelled" };

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentEvent"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <param name="amount">The payment amount.</param>
        /// <param name="currency">The payment currency.</param>
        /// <param name="method">The method of payment.</param>
        /// <param name="status">The payment status.</param>
        /// <param name="eventAt">The event timestamp in ISO 8601 format.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        public PaymentEvent(string id, int amount, string currency, string method, string status, string eventAt)
        {
            if (string.IsNullOrWhiteSpace(id) || amount <= 0 || string.IsNullOrWhiteSpace(currency) ||
                string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(eventAt))
            {
                throw new ArgumentException("All fields are required.");
            }

            if (!ValidStatuses.Contains(status))
            {
                throw new ArgumentException("Invalid status.");
            }

            if (!DateTimeOffset.TryParse(eventAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedOffset))
            {
                throw new ArgumentException("Invalid eventAt format.");
            }

            this.Id = id;
            this.Amount = amount;
            this.Currency = currency;
            this.Method = method;
            this.Status = status;
            this.EventAt = parsedOffset.UtcDateTime;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the payment event.
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// Gets or sets the amount associated with the payment.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency of the payment.
        /// </summary>
        public string Currency { get; set; } = default!;

        /// <summary>
        /// Gets or sets the payment method (e.g., credit_card, paypal).
        /// </summary>
        public string Method { get; set; } = default!;

        /// <summary>
        /// Gets or sets the status of the payment (e.g., paid, failed).
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// Gets or sets the UTC timestamp indicating when the payment event occurred.
        /// </summary>
        public DateTimeOffset EventAt { get; set; }
    }
}
