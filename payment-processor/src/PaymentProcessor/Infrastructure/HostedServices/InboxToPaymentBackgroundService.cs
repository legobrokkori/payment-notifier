namespace PaymentProcessor.Infrastructure.BackgroundServices
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;

    using PaymentProcessor.Application.Workers;

    /// <summary>
    /// Background service that continuously processes pending inbox events
    /// and converts them to domain payment events.
    /// </summary>
    public class InboxToPaymentBackgroundService : BackgroundService
    {
        private readonly InboxToPaymentWorker worker;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboxToPaymentBackgroundService"/> class.
        /// </summary>
        /// <param name="worker">The worker that processes pending inbox events.</param>
        public InboxToPaymentBackgroundService(InboxToPaymentWorker worker)
        {
            this.worker = worker;
        }

        /// <summary>
        /// Executes the background service by delegating to the worker.
        /// </summary>
        /// <param name="stoppingToken">A token that signals when the service should stop.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return this.worker.ProcessPendingEventsAsync(stoppingToken);
        }
    }
}
