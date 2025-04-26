// <copyright file="RedisPaymentEventSource.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Redis
{
    using System.Text.Json;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using PaymentProcessor.Domain.Entities;
    using PaymentProcessor.Domain.Factories;
    using PaymentProcessor.Domain.Interfaces;
    using PaymentProcessor.Infrastructure.Configurations;

    using StackExchange.Redis;

    /// <summary>
    /// Consumes payment events from Redis Stream using Consumer Group.
    /// </summary>
    public class RedisPaymentEventSource : IPaymentEventSource
    {
        // Read from the beginning of the stream (used when creating a new consumer group)
        private const string RedisStreamStart = "0-0";

        // Read only new messages (used when consuming events normally)
        private const string RedisStreamNewMessages = ">";
        private readonly RedisSettings settings;
        private readonly ILogger<RedisPaymentEventSource> logger;
        private readonly IDatabase redis;
        private readonly string consumerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisPaymentEventSource"/> class.
        /// </summary>
        /// <param name="options">Redis settings.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="connection">Redis connection multiplexer.</param>
        /// <param name="consumerName">Consumer group name (optional).</param>
        public RedisPaymentEventSource(
            IOptions<RedisSettings> options,
            ILogger<RedisPaymentEventSource> logger,
            IConnectionMultiplexer connection,
            string? consumerName = null)
        {
            this.settings = options.Value;
            this.logger = logger;
            this.redis = connection.GetDatabase();
            this.consumerName = consumerName ?? Environment.MachineName ?? $"consumer-{Guid.NewGuid()}";
        }

        /// <summary>
        /// Attempts to dequeue a payment event from Redis Stream using a consumer group.
        /// Validates and acknowledges the event if processing succeeds.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The deserialized <see cref="PaymentEvent"/>, or null if none or invalid.</returns>
        public async Task<PaymentEvent?> DequeueAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this.EnsureConsumerGroupAsync(cancellationToken);

                var entry = await this.ReadOneEntryAsync(cancellationToken);
                if (entry is null)
                {
                    return null;
                }

                var streamEntry = entry.Value;
                var paymentEvent = this.ParseEvent(streamEntry);
                if (paymentEvent is null)
                {
                    return null;
                }

                await this.AcknowledgeAsync(streamEntry.Id, cancellationToken);
                return paymentEvent;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error processing message from Redis Stream");
                return null;
            }
        }

        /// <summary>
        /// Ensures the consumer group exists for the given stream.
        /// If the group already exists, it silently continues.
        /// </summary>
        private async Task EnsureConsumerGroupAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this.redis.StreamCreateConsumerGroupAsync(
                    this.settings.Queue,
                    this.settings.Group,
                    RedisStreamStart, // "0-0": read from the beginning when the group is created
                    createStream: true);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                // Group already exists, no action needed
            }
        }

        /// <summary>
        /// Reads a single message from the Redis stream via XREADGROUP command.
        /// Only new messages (">") are considered.
        /// </summary>
        /// <returns>The next <see cref="StreamEntry"/> or null if no message is found.</returns>
        private async Task<StreamEntry?> ReadOneEntryAsync(CancellationToken cancellationToken)
        {
            var entries = await this.redis.StreamReadGroupAsync(
                this.settings.Queue,
                groupName: this.settings.Group,
                consumerName: this.consumerName,
                position: RedisStreamNewMessages, // ">": new messages only
                count: this.settings.ReadCount);

            return entries.Length > 0 ? entries[0] : null;
        }

        /// <summary>
        /// Attempts to parse the raw stream payload into a <see cref="PaymentEvent"/>.
        /// Logs warnings if parsing or validation fails.
        /// </summary>
        /// <param name="entry">The raw Redis stream entry.</param>
        /// <returns>A valid <see cref="PaymentEvent"/> or null if invalid.</returns>
        private PaymentEvent? ParseEvent(StreamEntry entry)
        {
            var payload = entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

            var result = PaymentEventFactory.TryCreate(
                id: payload.GetValueOrDefault("id") ?? string.Empty,
                amount: int.TryParse(payload.GetValueOrDefault("amount"), out var amt) ? amt : 0,
                currency: payload.GetValueOrDefault("currency") ?? string.Empty,
                method: payload.GetValueOrDefault("method") ?? string.Empty,
                status: payload.GetValueOrDefault("status") ?? string.Empty,
                eventAt: payload.GetValueOrDefault("eventAt") ?? string.Empty);

            if (!result.IsSuccess)
            {
                this.logger.LogWarning("Invalid payment event: {Error}", result.Error);
                return null;
            }

            return result.Value;
        }

        /// <summary>
        /// Acknowledges the processed entry in the Redis stream to avoid re-delivery.
        /// </summary>
        /// <param name="entryId">The ID of the stream entry to acknowledge.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        private Task AcknowledgeAsync(string? entryId, CancellationToken cancellationToken)
        {
            if (entryId is null)
            {
                throw new ArgumentNullException(nameof(entryId));
            }

            return this.redis.StreamAcknowledgeAsync(
                this.settings.Queue,
                this.settings.Group,
                entryId);
        }
    }
}
