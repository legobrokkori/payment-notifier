namespace PaymentProcessor.Infrastructure.BackgroundServices
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;

    using PaymentProcessor.Application.Workers;

    /// <summary>
    /// Background service that dequeues payment events from Redis
    /// and stores them in the Inbox table for downstream processing.
    /// </summary>
    public class RedisInboxIngestBackgroundService : BackgroundService
    {
        private readonly RedisInboxIngestWorker worker;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisInboxIngestBackgroundService"/> class.
        /// </summary>
        /// <param name="worker">The worker that ingests events from Redis into the Inbox.</param>
        public RedisInboxIngestBackgroundService(RedisInboxIngestWorker worker)
        {
            this.worker = worker;
        }

        /// <summary>
        /// Executes the background service by running the ingestion worker.
        /// </summary>
        /// <param name="stoppingToken">A token that signals when the service should stop.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return this.worker.RunAsync(stoppingToken);
        }
    }
}
