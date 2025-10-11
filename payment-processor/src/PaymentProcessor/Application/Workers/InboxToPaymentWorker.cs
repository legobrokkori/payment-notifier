// Application/Workers/InboxToPaymentWorker.cs
namespace PaymentProcessor.Application.Workers
{
    using Microsoft.Extensions.Logging;

    using PaymentProcessor.Application.Services;

    /// <summary>
    /// Orchestrates processing of pending inbox events by delegating to IInboxEventProcessor.
    /// </summary>
    public class InboxToPaymentWorker
    {
        private readonly IInboxEventProcessor processor;
        private readonly ILogger<InboxToPaymentWorker> logger;

        public InboxToPaymentWorker(IInboxEventProcessor processor, ILogger<InboxToPaymentWorker> logger)
        {
            this.processor = processor;
            this.logger = logger;
        }

        public async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting inbox processing...");
            await this.processor.ProcessAsync(cancellationToken);
            this.logger.LogInformation("Finished inbox processing.");
        }
    }
}
