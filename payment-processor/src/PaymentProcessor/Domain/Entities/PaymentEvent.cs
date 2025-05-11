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
        /// Initializes a new instance of the <see cref="PaymentEvent"/> class.
        /// </summary>
        public PaymentEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentEvent"/> class.
        /// This constructor is internal and should only be used by the factory.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <param name="amount">The payment amount.</param>
        /// <param name="currency">The currency of the payment.</param>
        /// <param name="method">The payment method.</param>
        /// <param name="status">The payment status.</param>
        /// <param name="eventAt">The event timestamp (UTC).</param>
        internal PaymentEvent(
            string id,
            int amount,
            string currency,
            string method,
            string status,
            DateTimeOffset eventAt)
        {
            this.Id = id;
            this.Amount = amount;
            this.Currency = currency;
            this.Method = method;
            this.Status = status;
            this.EventAt = eventAt;
        }

        /// <summary>
        /// Gets the unique identifier for the payment event.
        /// </summary>
        required public string Id { get; init; }

        /// <summary>
        /// Gets the amount associated with the payment.
        /// </summary>
        required public int Amount { get; init; }

        /// <summary>
        /// Gets the currency of the payment.
        /// </summary>
        required public string Currency { get; init; }

        /// <summary>
        /// Gets the payment method (e.g., credit_card, paypal).
        /// </summary>
        required public string Method { get; init; }

        /// <summary>
        /// Gets the status of the payment (e.g., paid, failed).
        /// </summary>
        required public string Status { get; init; }

        /// <summary>
        /// Gets the UTC timestamp indicating when the payment event occurred.
        /// </summary>
        required public DateTimeOffset EventAt { get; init; }
    }
}
