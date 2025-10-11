using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Events;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.IntegrationTests.Infrastructure;

using Xunit;

namespace PaymentProcessor.IntegrationTests.Application.Workers;

/// <summary>
/// Integration tests for InboxToPaymentWorker.
/// Uses IClassFixture to share database across tests with transaction isolation.
/// </summary>
public class InboxToPaymentWorkerTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture;

    public InboxToPaymentWorkerTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Process_Valid_Payment_Successfully()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        // Use transaction for test isolation
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "event-success-" + Guid.NewGuid().ToString("N");
        var paymentId = "pay-" + Guid.NewGuid().ToString("N");
        var payload = $@"{{
            ""Id"":""{paymentId}"",
            ""Amount"":1000,
            ""Currency"":""USD"",
            ""Method"":""card"",
            ""Status"":""paid"",
            ""EventAt"":""2024-04-01T10:00:00Z""
        }}";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var updatedInbox = await dbContext.InboxEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        updatedInbox.Should().NotBeNull();
        updatedInbox!.Status.Should().Be(InboxEventStatus.Completed);

        // Verify payment was saved
        var payment = await dbContext.PaymentEventRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.EventId == paymentId);

        payment.Should().NotBeNull();
        payment!.Amount.Should().Be(1000);
        payment.Currency.Should().Be("USD");
        payment.Method.Should().Be("card");
        payment.Status.Should().Be("paid");

        await transaction.RollbackAsync(); // Clean up
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Mark_Invalid_JSON_As_Failed()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "event-invalid-json-" + Guid.NewGuid().ToString("N");
        var payload = "{ this is not valid json }";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var updatedInbox = await dbContext.InboxEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        updatedInbox.Should().NotBeNull();
        updatedInbox!.Status.Should().Be(InboxEventStatus.Failed);

        // Verify payment was NOT saved
        var paymentCount = await dbContext.PaymentEventRecords
            .AsNoTracking()
            .CountAsync();
        paymentCount.Should().Be(0);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Mark_Null_Payload_As_Failed()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "event-null-payload-" + Guid.NewGuid().ToString("N");
        var payload = "null";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var updatedInbox = await dbContext.InboxEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        updatedInbox.Should().NotBeNull();
        updatedInbox!.Status.Should().Be(InboxEventStatus.Failed);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Process_Multiple_Events()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var event1Id = "multi-event-1-" + Guid.NewGuid().ToString("N");
        var event2Id = "multi-event-2-" + Guid.NewGuid().ToString("N");
        var event3Id = "multi-event-3-" + Guid.NewGuid().ToString("N");

        var payload1 = $@"{{""Id"":""pay-1"",""Amount"":100,""Currency"":""USD"",""Method"":""card"",""Status"":""paid"",""EventAt"":""2024-01-01T00:00:00Z""}}";
        var payload2 = $@"{{""Id"":""pay-2"",""Amount"":200,""Currency"":""EUR"",""Method"":""paypal"",""Status"":""paid"",""EventAt"":""2024-01-02T00:00:00Z""}}";
        var payload3 = "invalid json"; // This should fail

        dbContext.InboxEvents.AddRange(
            InboxEvent.CreatePending(event1Id, payload1),
            InboxEvent.CreatePending(event2Id, payload2),
            InboxEvent.CreatePending(event3Id, payload3)
        );
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var events = await dbContext.InboxEvents
            .AsNoTracking()
            .Where(e => e.EventId.StartsWith("multi-event-"))
            .ToListAsync();

        events.Should().HaveCount(3);
        events.Count(e => e.Status == InboxEventStatus.Completed).Should().Be(2);
        events.Count(e => e.Status == InboxEventStatus.Failed).Should().Be(1);

        var payments = await dbContext.PaymentEventRecords
            .AsNoTracking()
            .Where(p => p.EventId.StartsWith("pay-"))
            .ToListAsync();

        payments.Should().HaveCount(2);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Not_Process_Already_Completed_Events()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "event-already-completed-" + Guid.NewGuid().ToString("N");
        var payload = $@"{{""Id"":""pay-completed"",""Amount"":500,""Currency"":""USD"",""Method"":""card"",""Status"":""paid"",""EventAt"":""2024-01-01T00:00:00Z""}}";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        inbox.MarkCompleted(); // Already completed
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert - Should not create duplicate payment
        var payments = await dbContext.PaymentEventRecords
            .AsNoTracking()
            .Where(p => p.EventId == "pay-completed")
            .ToListAsync();

        payments.Should().BeEmpty("already completed events should not be reprocessed");

        await transaction.RollbackAsync();
    }
}
