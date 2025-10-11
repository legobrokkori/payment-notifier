using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PaymentProcessor.Application.Workers;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Events;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.IntegrationTests.Infrastructure;

using Xunit;

namespace PaymentProcessor.IntegrationTests.Application.Workers;

/// <summary>
/// Integration tests to verify that processing inbox events persists audit log entries.
/// Uses IClassFixture to share database across tests with transaction isolation.
/// </summary>
public class InboxToPaymentWorkerAuditLogTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture;

    public InboxToPaymentWorkerAuditLogTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Persist_AuditLog_For_Success()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        Environment.SetEnvironmentVariable("WORKER_NAME", "test-worker-success");

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "audit-success-" + Guid.NewGuid().ToString("N");
        var paymentId = "pay-" + Guid.NewGuid().ToString("N");
        var payload = $@"{{
            ""Id"":""{paymentId}"",
            ""Amount"":123,
            ""Currency"":""JPY"",
            ""Method"":""card"",
            ""Status"":""paid"",
            ""EventAt"":""2025-10-11T00:00:00Z""
        }}";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var logs = await dbContext.InboxEventLogs
            .AsNoTracking()
            .Where(l => l.InboxEventId == eventId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();

        logs.Should().NotBeEmpty("processing an inbox event should write audit logs");

        var log = logs.First();
        log.NewStatus.Should().Be(InboxEventStatus.Completed.ToString());
        log.OldStatus.Should().Be(InboxEventStatus.Pending.ToString());
        log.IsSuccess.Should().BeTrue();
        log.AttemptNo.Should().Be(1);
        log.EventType.Should().Be("PaymentEvent");
        log.WorkerNode.Should().Be("test-worker-success");
        log.ErrorCode.Should().BeNullOrEmpty();
        log.ErrorMessage.Should().BeNullOrEmpty();
        log.DurationMs.Should().BeGreaterThanOrEqualTo(0);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Persist_AuditLog_For_Failure()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        Environment.SetEnvironmentVariable("WORKER_NAME", "test-worker-failure");

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "audit-failure-" + Guid.NewGuid().ToString("N");
        var payload = "{ invalid json }";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var logs = await dbContext.InboxEventLogs
            .AsNoTracking()
            .Where(l => l.InboxEventId == eventId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();

        logs.Should().NotBeEmpty("processing a failed event should write audit logs");

        var log = logs.First();
        log.NewStatus.Should().Be(InboxEventStatus.Failed.ToString());
        log.OldStatus.Should().Be(InboxEventStatus.Pending.ToString());
        log.IsSuccess.Should().BeFalse();
        log.AttemptNo.Should().Be(1);
        log.WorkerNode.Should().Be("test-worker-failure");
        log.ErrorCode.Should().NotBeNullOrEmpty("failed events should have error code");
        log.ErrorMessage.Should().NotBeNullOrEmpty("failed events should have error message");
        log.DurationMs.Should().BeGreaterThanOrEqualTo(0);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Persist_AuditLog_For_Null_Payload()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        Environment.SetEnvironmentVariable("WORKER_NAME", "test-worker-null");

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "audit-null-" + Guid.NewGuid().ToString("N");
        var payload = "null";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var logs = await dbContext.InboxEventLogs
            .AsNoTracking()
            .Where(l => l.InboxEventId == eventId)
            .ToListAsync();

        logs.Should().NotBeEmpty();

        var log = logs.First();
        log.NewStatus.Should().Be(InboxEventStatus.Failed.ToString());
        log.IsSuccess.Should().BeFalse();
        log.ErrorCode.Should().Be("INVALID_PAYLOAD");
        log.ErrorMessage.Should().Be("Payload deserialization returned null");

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Create_Separate_AuditLogs_For_Multiple_Events()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        Environment.SetEnvironmentVariable("WORKER_NAME", "test-worker-multi");

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var event1Id = "audit-multi-1-" + Guid.NewGuid().ToString("N");
        var event2Id = "audit-multi-2-" + Guid.NewGuid().ToString("N");

        var payload1 = $@"{{""Id"":""pay-multi-1"",""Amount"":100,""Currency"":""USD"",""Method"":""card"",""Status"":""paid"",""EventAt"":""2024-01-01T00:00:00Z""}}";
        var payload2 = "invalid";

        dbContext.InboxEvents.AddRange(
            InboxEvent.CreatePending(event1Id, payload1),
            InboxEvent.CreatePending(event2Id, payload2)
        );
        await dbContext.SaveChangesAsync();

        // Act
        await worker.ProcessPendingEventsAsync(CancellationToken.None);

        // Assert
        var logs1 = await dbContext.InboxEventLogs
            .AsNoTracking()
            .Where(l => l.InboxEventId == event1Id)
            .ToListAsync();

        var logs2 = await dbContext.InboxEventLogs
            .AsNoTracking()
            .Where(l => l.InboxEventId == event2Id)
            .ToListAsync();

        logs1.Should().HaveCount(1, "one log per event");
        logs2.Should().HaveCount(1, "one log per event");

        logs1.First().IsSuccess.Should().BeTrue();
        logs2.First().IsSuccess.Should().BeFalse();

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ProcessPendingEventsAsync_Should_Record_Accurate_Duration()
    {
        // Arrange
        using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<InboxToPaymentWorker>();

        Environment.SetEnvironmentVariable("WORKER_NAME", "test-worker-duration");

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var eventId = "audit-duration-" + Guid.NewGuid().ToString("N");
        var paymentId = "pay-duration-" + Guid.NewGuid().ToString("N");
        var payload = $@"{{""Id"":""{paymentId}"",""Amount"":999,""Currency"":""USD"",""Method"":""card"",""Status"":""paid"",""EventAt"":""2024-01-01T00:00:00Z""}}";

        var inbox = InboxEvent.CreatePending(eventId, payload);
        dbContext.InboxEvents.Add(inbox);
        await dbContext.SaveChangesAsync();

        // Act
        var startTime = DateTimeOffset.UtcNow;
        await worker.ProcessPendingEventsAsync(CancellationToken.None);
        var endTime = DateTimeOffset.UtcNow;

        // Assert
        var log = await dbContext.InboxEventLogs
            .AsNoTracking()
            .FirstAsync(l => l.InboxEventId == eventId);

        log.DurationMs.Should().BeGreaterThanOrEqualTo(0);
        log.DurationMs.Should().BeLessThan((long)((endTime - startTime).TotalMilliseconds + 100),
            "recorded duration should be within reasonable bounds");

        await transaction.RollbackAsync();
    }
}
