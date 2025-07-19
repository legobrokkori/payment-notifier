using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Repositories;

namespace PaymentProcessor.Application.Services;

public class InboxEventProcessor : IInboxEventProcessor
{
    private readonly IInboxEventRepository inboxRepo;
    private readonly IPaymentRepository paymentRepo;
    private readonly ILogger<InboxEventProcessor> logger;

    public InboxEventProcessor(
        IInboxEventRepository inboxRepo,
        IPaymentRepository paymentRepo,
        ILogger<InboxEventProcessor> logger)
    {
        this.inboxRepo = inboxRepo;
        this.paymentRepo = paymentRepo;
        this.logger = logger;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var events = await this.inboxRepo.DequeuePendingAsync(10, cancellationToken);

        foreach (var inbox in events)
        {
            try
            {
                var payment = JsonSerializer.Deserialize<PaymentEvent>(inbox.RawPayload);
                if (payment == null)
                {
                    this.logger.LogWarning("Invalid payload for {EventId}", inbox.EventId);
                    inbox.MarkFailed();
                    continue;
                }

                await this.paymentRepo.SaveAsync(payment, cancellationToken);
                inbox.MarkCompleted();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to process event {EventId}", inbox.EventId);
                inbox.MarkFailed();
            }
        }

        await this.inboxRepo.SaveAsync(cancellationToken);
    }
}
