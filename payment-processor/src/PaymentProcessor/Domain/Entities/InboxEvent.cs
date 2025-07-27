using System;
using PaymentProcessor.Domain.Events;

namespace PaymentProcessor.Domain.Entities;

/// <summary>
/// Domain entity representing an event received from the inbox stream.
/// </summary>
public class InboxEvent
{
    /// <summary>
    /// Gets the unique identifier of the inbox event.
    /// </summary>
    required public string EventId { get; init; }

    /// <summary>
    /// Gets the raw payload of the event (e.g., JSON or Protobuf).
    /// </summary>
    required public string RawPayload { get; init; }

    /// <summary>
    /// Gets the status of the inbox event.
    /// </summary>
    public InboxEventStatus Status { get; private set; }

    /// <summary>
    /// Gets the timestamp when this event was created.
    /// </summary>
    required public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when this event was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    public static InboxEvent CreatePending(string eventId, string rawPayload)
    {
        return new InboxEvent
        {
            EventId = eventId,
            RawPayload = rawPayload,
            Status = InboxEventStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void MarkCompleted() => this.Status = InboxEventStatus.Completed;

    public void MarkFailed() => this.Status = InboxEventStatus.Failed;

    public void MarkAsCompleted()
    {
        this.Status = InboxEventStatus.Completed;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        this.Status = InboxEventStatus.Processing;
        this.UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        this.Status = InboxEventStatus.Failed;
        this.UpdatedAt = DateTime.UtcNow;
    }
}
