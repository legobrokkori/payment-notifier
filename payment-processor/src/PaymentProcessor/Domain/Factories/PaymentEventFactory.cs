// <copyright file="PaymentEventFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Domain.Factories
{
    using System;
    using System.Globalization;
    using System.Linq;

    using PaymentProcessor.Domain.Entities;
    using PaymentProcessor.Domain.Shared;

    /// <summary>
    /// Factory class for creating <see cref="PaymentEvent"/> instances with validation.
    /// </summary>
    public static class PaymentEventFactory
    {
        /// <summary>
        /// List of valid status values accepted by the domain.
        /// </summary>
        private static readonly string[] ValidStatuses = { "paid", "failed", "cancelled" };

        /// <summary>
        /// Tries to create a new instance of <see cref="PaymentEvent"/> from the provided input.
        /// Performs validation and returns a <see cref="Result{T}"/> indicating success or failure.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <param name="amount">The payment amount (must be greater than 0).</param>
        /// <param name="currency">The ISO 4217 currency code.</param>
        /// <param name="method">The payment method (e.g., credit_card, paypal).</param>
        /// <param name="status">The payment status (must be one of the valid statuses).</param>
        /// <param name="eventAt">The timestamp of the event in ISO 8601 format.</param>
        /// <returns>A <see cref="Result{T}"/> containing a valid <see cref="PaymentEvent"/> or an error message.</returns>
        public static Result<PaymentEvent> TryCreate(
            string id,
            int amount,
            string currency,
            string method,
            string status,
            string eventAt)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                amount <= 0 ||
                string.IsNullOrWhiteSpace(currency) ||
                string.IsNullOrWhiteSpace(method) ||
                string.IsNullOrWhiteSpace(status) ||
                string.IsNullOrWhiteSpace(eventAt))
            {
                return Result<PaymentEvent>.Failure("All fields are required.");
            }

            if (!ValidStatuses.Contains(status))
            {
                return Result<PaymentEvent>.Failure("Invalid status.");
            }

            if (!DateTimeOffset.TryParse(eventAt, null, DateTimeStyles.RoundtripKind, out var parsedOffset))
            {
                return Result<PaymentEvent>.Failure("Invalid eventAt format.");
            }

            var entity = new PaymentEvent(id, amount, currency, method, status, parsedOffset.UtcDateTime);
            return Result<PaymentEvent>.Success(entity);
        }
    }
}
