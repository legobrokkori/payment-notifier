namespace PaymentProcessor.Domain.Entities;

public class PaymentEvent
{
    public string Id { get; set; } = default!;
    public int Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime OccurredAt { get; set; }
}