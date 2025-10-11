using System;
using PaymentProcessor.Domain.Events;
using PaymentProcessor.Infrastructure.Persistence.Auditing;

namespace PaymentProcessor.Domain.Entities;

/// <summary>
/// Domain entity representing an event received from the inbox stream.
/// </summary>
public class InboxEvent : IAuditableEntity
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
    /// Gets or sets the timestamp when this event was created.
    /// Automatically set by EF Core on insert.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this event was last updated.
    /// Automatically set by EF Core on insert/update.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    public static InboxEvent CreatePending(string eventId, string rawPayload)
    {
        return new InboxEvent
        {
            EventId = eventId,
            RawPayload = rawPayload,
            Status = InboxEventStatus.Pending,
        };
    }

    /// <summary>
    /// Marks the event as successfully completed.
    /// UpdatedAt will be automatically set by EF Core.
    /// </summary>
    public void MarkCompleted()
    {
        this.Status = InboxEventStatus.Completed;
    }

    /// <summary>
    /// Marks the event as failed.
    /// UpdatedAt will be automatically set by EF Core.
    /// </summary>
    public void MarkFailed()
    {
        this.Status = InboxEventStatus.Failed;
    }

    /// <summary>
    /// Marks the event as currently being processed.
    /// UpdatedAt will be automatically set by EF Core.
    /// </summary>
    public void MarkAsProcessing()
    {
        this.Status = InboxEventStatus.Processing;
    }
}
