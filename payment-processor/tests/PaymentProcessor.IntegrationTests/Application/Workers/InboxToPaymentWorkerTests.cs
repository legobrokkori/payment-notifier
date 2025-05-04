using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Events;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;
using PaymentProcessor.IntegrationTests.Infrastructure;

using Xunit;

namespace PaymentProcessor.IntegrationTests.Application.Workers;

public class InboxToPaymentWorkerTests : IAsyncLifetime
{
    private readonly AppDbContext dbContext;
    private readonly InboxToPaymentWorker worker;

    public InboxToPaymentWorkerTests()
    {
        var serviceProvider = TestAppFactory.Create();
        this.dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<InboxToPaymentWorker>>();
        this.worker = new InboxToPaymentWorker(dbContext, logger);
    }

    public async Task InitializeAsync()
    {
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.InboxEvents.AddRange(
            new InboxEvent
            {
                EventId = "event-success",
                RawPayload = @"{""id"":""abc"",""amount"":1000,""currency"":""USD"",""method"":""card"",""status"":""paid"",""eventAt"":""2024-04-01T10:00:00Z""}",
                Status = InboxEventStatus.Pending.ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InboxEvent
            {
                EventId = "event-invalid-json",
                RawPayload = @"{ this is not valid json }",
                Status = InboxEventStatus.Pending.ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InboxEvent
            {
                EventId = "event-null-payload",
                RawPayload = "null",
                Status = InboxEventStatus.Pending.ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        );

        await dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Update_Status_Correctly()
    {
        // Act
        await worker.ProcessPendingEventsAsync(default);

        // Assert
        dbContext.InboxEvents.First(e => e.EventId == "event-success").Status.Should().Be(InboxEventStatus.Completed.ToString());
        dbContext.InboxEvents.First(e => e.EventId == "event-invalid-json").Status.Should().Be(InboxEventStatus.Failed.ToString());
        dbContext.InboxEvents.First(e => e.EventId == "event-null-payload").Status.Should().Be(InboxEventStatus.Failed.ToString());
    }
}
