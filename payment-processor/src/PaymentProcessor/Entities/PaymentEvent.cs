namespace PaymentProcessor.Domain.Entities;

public class PaymentEvent
{
    public string Id { get; set; } = default!;
    public int Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset EventAt { get; set; }

    private static readonly string[] ValidStatuses = { "paid", "failed", "cancelled" };

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

        Id = id;
        Amount = amount;
        Currency = currency;
        Method = method;
        Status = status;
        EventAt = parsedOffset.UtcDateTime;
    }
}