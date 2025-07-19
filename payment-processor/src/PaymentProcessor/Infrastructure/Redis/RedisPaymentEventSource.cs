// <copyright file="RedisPaymentEventSource.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Redis
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Payment;

    using PaymentProcessor.Application.Mappers;
    using PaymentProcessor.Domain.Entities;
    using PaymentProcessor.Domain.Interfaces;
    using PaymentProcessor.Infrastructure.Configurations;

    using StackExchange.Redis;

    using DomainPaymentEvent = PaymentProcessor.Domain.Entities.PaymentEvent;
    using ProtoPaymentEvent = Payment.PaymentEvent;

    /// <summary>
    /// Consumes payment events from Redis Stream using a consumer group.
    /// </summary>
    public class RedisPaymentEventSource : IPaymentEventSource
    {
        private const string RedisStreamStart = "0-0";
        private const string RedisStreamNewMessages = ">";

        private readonly RedisSettings settings;
        private readonly ILogger<RedisPaymentEventSource> logger;
        private readonly IDatabase redis;
        private readonly string consumerName;

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

        public async Task<DomainPaymentEvent?> DequeueAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this.EnsureConsumerGroupAsync();

                var entry = await this.ReadOneEntryAsync();
                if (entry is null)
                {
                    return null;
                }

                var dataField = entry.Value.Values.FirstOrDefault(x => x.Name == "data");
                if (!dataField.Value.HasValue || !dataField.Value.TryGetValue(out byte[] ? rawBytes))
                {
                    this.logger.LogWarning("Stream entry missing or invalid 'data' field. EntryId={EntryId}", entry.Value.Id);
                    return null;
                }

                ProtoPaymentEvent protoEvent;
                try
                {
                    protoEvent = ProtoPaymentEvent.Parser.ParseFrom(rawBytes);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to parse Protobuf. EntryId={EntryId}", entry.Value.Id);
                    return null;
                }

                var result = PaymentEventMapper.FromProto(protoEvent);
                if (!result.IsSuccess)
                {
                    this.logger.LogWarning("Invalid domain event: {Error}. EntryId={EntryId}", result.Error, entry.Value.Id);
                    return null;
                }

                await this.AcknowledgeAsync(entry.Value.Id);
                return result.Value;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error during DequeueAsync");
                return null;
            }
        }

        private async Task EnsureConsumerGroupAsync()
        {
            try
            {
                await this.redis.StreamCreateConsumerGroupAsync(
                    this.settings.Queue,
                    this.settings.Group,
                    RedisStreamStart,
                    createStream: true);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                // Group already exists — no action needed
            }
        }

        private async Task<StreamEntry?> ReadOneEntryAsync()
        {
            var entries = await this.redis.StreamReadGroupAsync(
                this.settings.Queue,
                this.settings.Group,
                this.consumerName,
                RedisStreamNewMessages,
                count: this.settings.ReadCount);

            return entries.Length > 0 ? entries[0] : null;
        }

        private Task AcknowledgeAsync(RedisValue entryId)
        {
            return this.redis.StreamAcknowledgeAsync(
                this.settings.Queue,
                this.settings.Group,
                entryId);
        }
    }
}
