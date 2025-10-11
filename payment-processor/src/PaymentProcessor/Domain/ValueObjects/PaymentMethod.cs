using System;
using System.Collections.Generic;

namespace PaymentProcessor.Domain.ValueObjects;

/// <summary>
/// Value object representing payment method.
/// </summary>
public record PaymentMethod
{
    private static readonly HashSet<string> ValidMethods = new()
    {
        "card", "bank_transfer", "paypal", "crypto", "cash"
    };

    /// <summary>
    /// Gets the payment method value.
    /// </summary>
    public string Value { get; }

    private PaymentMethod(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a PaymentMethod from a string value.
    /// </summary>
    /// <param name="value">The payment method string.</param>
    /// <returns>A PaymentMethod instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the payment method is invalid.</exception>
    public static PaymentMethod From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Payment method cannot be empty", nameof(value));
        }

        var lowerValue = value.ToLowerInvariant();
        if (!ValidMethods.Contains(lowerValue))
        {
            throw new ArgumentException($"Invalid payment method: {value}", nameof(value));
        }

        return new PaymentMethod(lowerValue);
    }

    /// <summary>
    /// Gets the Card payment method.
    /// </summary>
    public static PaymentMethod Card => new("card");

    /// <summary>
    /// Gets the Bank Transfer payment method.
    /// </summary>
    public static PaymentMethod BankTransfer => new("bank_transfer");

    /// <summary>
    /// Gets the PayPal payment method.
    /// </summary>
    public static PaymentMethod PayPal => new("paypal");

    /// <summary>
    /// Gets the Crypto payment method.
    /// </summary>
    public static PaymentMethod Crypto => new("crypto");

    /// <summary>
    /// Gets the Cash payment method.
    /// </summary>
    public static PaymentMethod Cash => new("cash");

    public override string ToString() => Value;
}
