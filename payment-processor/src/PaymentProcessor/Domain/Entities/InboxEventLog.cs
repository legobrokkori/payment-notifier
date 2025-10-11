using System;

namespace PaymentProcessor.Domain.Entities;

/// <summary>
/// Represents an immutable audit log entry for an InboxEvent.
/// Each state transition or processing attempt appends one new row.
/// This enables traceability, troubleshooting, and compliance.
/// </summary>
public sealed class InboxEventLog
{
    /// <summary>
    /// Unique identifier of this audit log entry.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// The foreign key referencing the associated InboxEvent.
    /// </summary>
    public string InboxEventId { get; private set; } = default!;

    /// <summary>
    /// Navigation property to the parent InboxEvent.
    /// </summary>
    public InboxEvent? InboxEvent { get; private set; }

    /// <summary>
    /// Optional type of the event (e.g., PaymentLinked, PaymentUnlinked).
    /// </summary>
    public string? EventType { get; private set; }

    /// <summary>
    /// Status before this transition (for traceability).
    /// </summary>
    public string? OldStatus { get; private set; }

    /// <summary>
    /// Status after this transition.
    /// </summary>
    public string NewStatus { get; private set; } = default!;

    /// <summary>
    /// The sequential attempt number (1, 2, 3...).
    /// </summary>
    public int AttemptNo { get; private set; }

    /// <summary>
    /// Whether this attempt was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Optional error code when the attempt failed.
    /// </summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// Optional error message when the attempt failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Worker node identifier that processed this event (useful in distributed systems).
    /// </summary>
    public string? WorkerNode { get; private set; }

    /// <summary>
    /// Correlation ID for cross-service tracing if available.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Processing duration in milliseconds (optional).
    /// </summary>
    public long? DurationMs { get; private set; }

    /// <summary>
    /// Timestamp when this log entry was created (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private InboxEventLog() { } // Required by EF Core

    /// <summary>
    /// Factory method to create a new audit log entry.
    /// </summary>
    public static InboxEventLog Create(
        string inboxEventId,
        string newStatus,
        int attemptNo,
        bool isSuccess,
        string? eventType = null,
        string? oldStatus = null,
        string? errorCode = null,
        string? errorMessage = null,
        string? workerNode = null,
        string? correlationId = null,
        long? durationMs = null)
    {
        return new InboxEventLog
        {
            InboxEventId = inboxEventId,
            NewStatus = newStatus,
            AttemptNo = attemptNo,
            IsSuccess = isSuccess,
            EventType = eventType,
            OldStatus = oldStatus,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            WorkerNode = workerNode,
            CorrelationId = correlationId,
            DurationMs = durationMs,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
