using System;

namespace PaymentProcessor.Domain.ValueObjects;

/// <summary>
/// Value object representing monetary amount with currency.
/// </summary>
public record Money
{
    /// <summary>
    /// Gets the amount in minor units (e.g., cents for USD).
    /// </summary>
    public int Amount { get; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The amount in minor units.</param>
    /// <param name="currency">The currency.</param>
    /// <exception cref="ArgumentException">Thrown when amount is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when currency is null.</exception>
    public Money(int amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    /// <summary>
    /// Adds two money values if they have the same currency.
    /// </summary>
    /// <param name="other">The other money value to add.</param>
    /// <returns>A new Money instance with the sum.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot add money with different currencies: {Currency} and {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Checks if this money value is greater than another.
    /// </summary>
    /// <param name="other">The other money value to compare.</param>
    /// <returns>True if this amount is greater; otherwise false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot compare money with different currencies: {Currency} and {other.Currency}");
        }

        return Amount > other.Amount;
    }

    public override string ToString() => $"{Amount} {Currency.Code}";
}
