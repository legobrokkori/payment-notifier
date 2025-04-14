namespace PaymentProcessor.Domain.Entities;

public class PaymentEvent
{
    public string Id { get; set; } = default!;
    public int Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime OccurredAt { get; set; }

    public PaymentEvent(string id, int amount, string currency, string method, string status, DateTime occurredAt)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id is required.", nameof(id));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Method is required.", nameof(method));

        if (!IsValidStatus(status))
            throw new ArgumentException($"Invalid status: {status}", nameof(status));

        Id = id;
        Amount = amount;
        Currency = currency;
        Method = method;
        Status = status;
        OccurredAt = occurredAt;
    }

    private bool IsValidStatus(string status)
    {
        // Define valid statuses here
        return new[] { "paid", "failed", "cancelled" }.Contains(status.ToLowerInvariant());
    }
}