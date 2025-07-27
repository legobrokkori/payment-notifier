// RedisInboxIngestWorkerTests - Boundary and Exception Tests
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Interfaces;
using PaymentProcessor.Domain.Repositories;

using Xunit;

namespace PaymentProcessor.Tests.Application.Workers;

public class RedisInboxIngestWorkerTests
{
    [Fact]
    public async Task RunAsync_Should_Not_Save_When_No_Event()
    {
        var eventSourceMock = new Mock<IPaymentEventSource>();
        var inboxRepositoryMock = new Mock<PaymentProcessor.Domain.Repositories.IInboxEventRepository>();
        var loggerMock = new Mock<ILogger<RedisInboxIngestWorker>>();

        eventSourceMock.Setup(s => s.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEvent?)null);

        var worker = new RedisInboxIngestWorker(eventSourceMock.Object, inboxRepositoryMock.Object, loggerMock.Object);
        await worker.RunAsync(CancellationToken.None);

        inboxRepositoryMock.Verify(r => r.TrySaveAsync(It.IsAny<InboxEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_Should_Stop_After_MaxMessages()
    {
        var eventSourceMock = new Mock<IPaymentEventSource>();
        var inboxRepositoryMock = new Mock<PaymentProcessor.Domain.Repositories.IInboxEventRepository>();
        var loggerMock = new Mock<ILogger<RedisInboxIngestWorker>>();

        var paymentEvent = new PaymentEvent
        {
            Id = "event-abc",
            Amount = 500,
            Currency = "USD",
            Method = "card",
            Status = "paid",
            EventAt = DateTimeOffset.UtcNow,
        };
        var sequence = eventSourceMock.SetupSequence(s => s.DequeueAsync(It.IsAny<CancellationToken>()));
        for (int i = 0; i < 100; i++)
            sequence = sequence.ReturnsAsync(paymentEvent);
        sequence.ReturnsAsync((PaymentEvent?)null);

        var worker = new RedisInboxIngestWorker(eventSourceMock.Object, inboxRepositoryMock.Object, loggerMock.Object);
        await worker.RunAsync(CancellationToken.None);

        inboxRepositoryMock.Verify(r => r.TrySaveAsync(It.IsAny<InboxEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(100));
    }

    [Fact]
    public async Task RunAsync_Should_Handle_Dequeue_Exception()
    {
        var eventSourceMock = new Mock<IPaymentEventSource>();
        var inboxRepositoryMock = new Mock<PaymentProcessor.Domain.Repositories.IInboxEventRepository>();
        var loggerMock = new Mock<ILogger<RedisInboxIngestWorker>>();

        eventSourceMock.Setup(s => s.DequeueAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Dequeue failed"));

        var worker = new RedisInboxIngestWorker(eventSourceMock.Object, inboxRepositoryMock.Object, loggerMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => worker.RunAsync(CancellationToken.None));
    }

    [Fact]
    public async Task RunAsync_Should_Handle_TrySaveAsync_Exception()
    {
        var paymentEvent = new PaymentEvent
        {
            Id = "event-def",
            Amount = 500,
            Currency = "USD",
            Method = "card",
            Status = "paid",
            EventAt = DateTimeOffset.UtcNow,
        };
        var eventSourceMock = new Mock<IPaymentEventSource>();
        var inboxRepositoryMock = new Mock<PaymentProcessor.Domain.Repositories.IInboxEventRepository>();
        var loggerMock = new Mock<ILogger<RedisInboxIngestWorker>>();

        eventSourceMock.SetupSequence(s => s.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentEvent)
            .ReturnsAsync((PaymentEvent?)null);

        inboxRepositoryMock.Setup(r => r.TrySaveAsync(It.IsAny<InboxEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var worker = new RedisInboxIngestWorker(eventSourceMock.Object, inboxRepositoryMock.Object, loggerMock.Object);

        await Assert.ThrowsAsync<Exception>(() => worker.RunAsync(CancellationToken.None));
    }

    [Fact]
    public async Task RunAsync_Should_Skip_Duplicate_EventId()
    {
        // Arrange: Create a payment event with the same EventId returned twice
        var paymentEvent = new PaymentEvent
        {
            Id = "event-duplicate",
            Amount = 500,
            Currency = "USD",
            Method = "card",
            Status = "paid",
            EventAt = DateTimeOffset.UtcNow,
        };

        var eventSourceMock = new Mock<IPaymentEventSource>();
        var inboxRepositoryMock = new Mock<PaymentProcessor.Domain.Repositories.IInboxEventRepository>();
        var loggerMock = new Mock<ILogger<RedisInboxIngestWorker>>();

        // Return the same event twice, followed by null to break the loop
        eventSourceMock.SetupSequence(s => s.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentEvent)
            .ReturnsAsync(paymentEvent)
            .ReturnsAsync((PaymentEvent?)null);

        // First save succeeds, second throws due to duplicate EventId (e.g., DB primary key constraint)
        inboxRepositoryMock.SetupSequence(r => r.TrySaveAsync(It.IsAny<InboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new InvalidOperationException("Duplicate EventId"));

        var worker = new RedisInboxIngestWorker(
            eventSourceMock.Object,
            inboxRepositoryMock.Object,
            loggerMock.Object);

        // Act
        await worker.RunAsync(CancellationToken.None);

        // Assert: TrySaveAsync should be called twice, second time causes failure due to duplicate
        inboxRepositoryMock.Verify(r => r.TrySaveAsync(
            It.IsAny<InboxEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Optionally verify that the logger captured the exception or warning
    }
}
