using System;
using System.Collections.Generic;

namespace PaymentProcessor.Domain.ValueObjects;

/// <summary>
/// Value object representing a currency with validation.
/// </summary>
public record Currency
{
    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "USD", "EUR", "JPY", "GBP", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD"
    };

    /// <summary>
    /// Gets the ISO 4217 currency code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class.
    /// </summary>
    /// <param name="code">The ISO 4217 currency code.</param>
    /// <exception cref="ArgumentException">Thrown when the currency code is invalid.</exception>
    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code cannot be empty", nameof(code));
        }

        var upperCode = code.ToUpperInvariant();
        if (!ValidCurrencies.Contains(upperCode))
        {
            throw new ArgumentException($"Invalid currency code: {code}", nameof(code));
        }

        Code = upperCode;
    }

    /// <summary>
    /// Gets USD currency.
    /// </summary>
    public static Currency USD => new("USD");

    /// <summary>
    /// Gets EUR currency.
    /// </summary>
    public static Currency EUR => new("EUR");

    /// <summary>
    /// Gets JPY currency.
    /// </summary>
    public static Currency JPY => new("JPY");

    /// <summary>
    /// Gets GBP currency.
    /// </summary>
    public static Currency GBP => new("GBP");

    public override string ToString() => Code;
}
