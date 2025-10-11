using System;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Repositories;

namespace PaymentProcessor.Application.Services;

public class InboxEventProcessor : IInboxEventProcessor
{
    private readonly IInboxEventRepository inboxRepo;
    private readonly IInboxEventLogRepository auditRepo;
    private readonly IPaymentRepository paymentRepo;
    private readonly ILogger<InboxEventProcessor> logger;

    public InboxEventProcessor(
        IInboxEventRepository inboxRepo,
        IInboxEventLogRepository auditRepo,
        IPaymentRepository paymentRepo,
        ILogger<InboxEventProcessor> logger)
    {
        this.inboxRepo = inboxRepo;
        this.auditRepo = auditRepo;
        this.paymentRepo = paymentRepo;
        this.logger = logger;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var worker = Environment.GetEnvironmentVariable("WORKER_NAME");
        var events = await this.inboxRepo.DequeuePendingAsync(10, cancellationToken);

        foreach (var inbox in events)
        {
            var sw = Stopwatch.StartNew();
            var oldStatus = inbox.Status.ToString();
            var attempt = 1; // TODO: compute attempt number properly

            try
            {
                var payment = JsonSerializer.Deserialize<PaymentEvent>(inbox.RawPayload);
                if (payment == null)
                {
                    this.logger.LogWarning("Invalid payload for {EventId}", inbox.EventId);
                    inbox.MarkFailed();
                    this.auditRepo.Add(InboxEventLog.Create(
                        inboxEventId: inbox.EventId,
                        newStatus: inbox.Status.ToString(),    // now "Failed"
                        attemptNo: attempt,
                        isSuccess: false,
                        eventType: payment?.GetType().Name,
                        oldStatus: oldStatus,
                        errorCode: "INVALID_PAYLOAD",
                        errorMessage: "Payload deserialization returned null",
                        workerNode: worker,
                        durationMs: sw.ElapsedMilliseconds
                    ));
                    continue;
                }

                await this.paymentRepo.SaveAsync(payment, cancellationToken);
                inbox.MarkCompleted();

                this.auditRepo.Add(InboxEventLog.Create(
                    inboxEventId: inbox.EventId,
                    newStatus: inbox.Status.ToString(),        // "Completed"
                    attemptNo: attempt,
                    isSuccess: true,
                    eventType: payment.GetType().Name,
                    oldStatus: oldStatus,
                    workerNode: worker,
                    durationMs: sw.ElapsedMilliseconds
                ));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to process event {EventId}", inbox.EventId);
                inbox.MarkFailed();

                this.auditRepo.Add(InboxEventLog.Create(
                    inboxEventId: inbox.EventId,
                    newStatus: inbox.Status.ToString(),        // "Failed"
                    attemptNo: attempt,
                    isSuccess: false,
                    eventType: null,
                    oldStatus: oldStatus,
                    errorCode: ex.GetType().Name,
                    errorMessage: ex.Message,
                    workerNode: worker,
                    durationMs: sw.ElapsedMilliseconds
                ));
            }
        }

        await this.inboxRepo.SaveAsync(cancellationToken);
    }
}
