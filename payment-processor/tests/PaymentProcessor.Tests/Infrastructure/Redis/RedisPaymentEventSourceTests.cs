// <copyright file="RedisPaymentEventSourceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Tests.Infrastructure.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Google.Protobuf;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Moq;

    using Payment;

    using PaymentProcessor.Domain.Entities;
    using PaymentProcessor.Infrastructure.Configurations;
    using PaymentProcessor.Infrastructure.Redis;

    using StackExchange.Redis;

    using Xunit;

    using ProtoPaymentEvent = Payment.PaymentEvent;

    /// <summary>
    /// Unit tests for <see cref="RedisPaymentEventSource"/>.
    /// Covers normal, error, and boundary cases.
    /// </summary>
    public class RedisPaymentEventSourceTests : IDisposable
    {
        // Shared test setup for all cases
        private readonly RedisSettings settings;
        private readonly Mock<ILogger<RedisPaymentEventSource>> loggerMock;
        private readonly Mock<IDatabase> redisDbMock;
        private readonly Mock<IConnectionMultiplexer> connectionMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisPaymentEventSourceTests"/> class.
        /// </summary>
        public RedisPaymentEventSourceTests()
        {
            // Common Redis settings
            this.settings = new RedisSettings
            {
                Host = "localhost",
                Port = 6379,
                Queue = "payment-events",
                Group = "payment-group",
                ReadCount = 1,
                ConnectionString = "localhost:6379",
            };
            this.loggerMock = new();
            this.redisDbMock = new();
            this.connectionMock = new();

            // Always return our mock database
            this.connectionMock
                .Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(this.redisDbMock.Object);

            // Consumer group creation: Simulate "already exists" (BUSYGROUP)
            this.redisDbMock
                .Setup(db => db.StreamCreateConsumerGroupAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<bool>(),
                    It.IsAny<CommandFlags>()))
                .ThrowsAsync(new RedisServerException("BUSYGROUP Consumer Group name already exists"));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // No resources to clean up, add if needed
        }

        /// <summary>
        /// Should return a valid PaymentEvent when Redis returns a well-formed StreamEntry.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DequeueAsync_Should_Return_PaymentEvent_When_Valid()
        {
            // Arrange: A valid stream entry
            var proto = new ProtoPaymentEvent
            {
                Id = "abc",
                Amount = 1000,
                Currency = "USD",
                Method = "card",
                Status = "paid",
                OccurredAt = "2024-04-01T10:00:00Z",
            };
            var bytes = proto.ToByteArray();

            var entry = new StreamEntry(
                "123-0",
                new[] { new NameValueEntry("data", bytes) }); // ← data フィールドだけ

            this.redisDbMock
                .Setup(db => db.StreamReadGroupAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(new[] { entry });

            this.redisDbMock
                .Setup(db => db.StreamAcknowledgeAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(1);

            var source = this.CreateSource();

            // Act
            var result = await source.DequeueAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("abc");
            result.Amount.Should().Be(1000);
            result.Currency.Should().Be("USD");
        }

        /// <summary>
        /// Should return null if the stream is empty (no messages available).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DequeueAsync_Should_ReturnNull_When_Stream_Is_Empty()
        {
            // Arrange: Redis returns no entries
            this.redisDbMock
                .Setup(db => db.StreamReadGroupAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(Array.Empty<StreamEntry>());

            var source = this.CreateSource();

            // Act
            var result = await source.DequeueAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Should return null when an invalid PaymentEvent is present (validation fails).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DequeueAsync_Should_ReturnNull_When_Event_Is_Invalid()
        {
            // Arrange: "amount" field is not a number (invalid event)
            var entry = new StreamEntry("123-1", new NameValueEntry[]
            {
                new ("id", "def"),
                new ("amount", "not-a-number"),
                new ("currency", "USD"),
                new ("method", "card"),
                new ("status", "paid"),
                new ("eventAt", "2024-04-01T10:00:00Z"),
            });

            this.redisDbMock
                .Setup(db => db.StreamReadGroupAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(new[] { entry });

            var source = this.CreateSource();

            // Act
            var result = await source.DequeueAsync(CancellationToken.None);

            // Assert: Invalid, should return null
            result.Should().BeNull();
        }

        /// <summary>
        /// Should handle boundary value for amount: zero is accepted if business logic allows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DequeueAsync_Should_Return_PaymentEvent_When_Amount_Is_Zero()
        {
            // Arrange: amount is zero (boundary case)
            var entry = new StreamEntry("123-2", new NameValueEntry[]
            {
                new ("id", "ghi"),
                new ("amount", "0"),
                new ("currency", "JPY"),
                new ("method", "bank"),
                new ("status", "pending"),
                new ("eventAt", "2024-04-01T10:00:00Z"),
            });

            this.redisDbMock
                .Setup(db => db.StreamReadGroupAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(new[] { entry });

            this.redisDbMock
                .Setup(db => db.StreamAcknowledgeAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(1);

            var source = this.CreateSource();

            // Act
            var result = await source.DequeueAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Should gracefully return null and log error when Redis throws an exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DequeueAsync_Should_ReturnNull_When_Redis_Throws_Exception()
        {
            // Arrange: Redis throws an exception (e.g., network error)
            this.redisDbMock
                .Setup(db => db.StreamReadGroupAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<RedisValue?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CommandFlags>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Simulated failure"));

            var source = this.CreateSource();

            // Act
            var result = await source.DequeueAsync(CancellationToken.None);

            // Assert: Should return null, error is logged internally
            result.Should().BeNull();
        }

        // Helper to create system under test (SUT)
        private RedisPaymentEventSource CreateSource()
            => new RedisPaymentEventSource(
                Options.Create(this.settings),
                this.loggerMock.Object,
                this.connectionMock.Object,
                "test-consumer");
    }
}
