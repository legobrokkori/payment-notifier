using System;
using System.Collections.Generic;

namespace PaymentProcessor.Domain.ValueObjects;

/// <summary>
/// Value object representing payment status.
/// </summary>
public record PaymentStatus
{
    private static readonly HashSet<string> ValidStatuses = new()
    {
        "paid", "failed", "cancelled", "pending", "refunded"
    };

    /// <summary>
    /// Gets the status value.
    /// </summary>
    public string Value { get; }

    private PaymentStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a PaymentStatus from a string value.
    /// </summary>
    /// <param name="value">The status string.</param>
    /// <returns>A PaymentStatus instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the status is invalid.</exception>
    public static PaymentStatus From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Payment status cannot be empty", nameof(value));
        }

        var lowerValue = value.ToLowerInvariant();
        if (!ValidStatuses.Contains(lowerValue))
        {
            throw new ArgumentException($"Invalid payment status: {value}", nameof(value));
        }

        return new PaymentStatus(lowerValue);
    }

    /// <summary>
    /// Gets the Paid status.
    /// </summary>
    public static PaymentStatus Paid => new("paid");

    /// <summary>
    /// Gets the Failed status.
    /// </summary>
    public static PaymentStatus Failed => new("failed");

    /// <summary>
    /// Gets the Cancelled status.
    /// </summary>
    public static PaymentStatus Cancelled => new("cancelled");

    /// <summary>
    /// Gets the Pending status.
    /// </summary>
    public static PaymentStatus Pending => new("pending");

    /// <summary>
    /// Gets the Refunded status.
    /// </summary>
    public static PaymentStatus Refunded => new("refunded");

    /// <summary>
    /// Checks if the payment was successful.
    /// </summary>
    /// <returns>True if the payment is in a successful state.</returns>
    public bool IsSuccessful() => this == Paid;

    /// <summary>
    /// Checks if the payment can be refunded.
    /// </summary>
    /// <returns>True if the payment can be refunded.</returns>
    public bool CanBeRefunded() => this == Paid;

    public override string ToString() => Value;
}
