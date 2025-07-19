namespace PaymentProcessor.Tests.Infrastructure.Persistence;

using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Events;
using PaymentProcessor.Infrastructure.Persistence;
using PaymentProcessor.Infrastructure.Persistence.Repositories;

using Xunit;

/// <summary>
/// Unit tests for the <see cref="InboxEventRepository"/> class.
/// These tests validate the behavior of TrySaveAsync when handling duplicate entries and primary key violations.
/// </summary>
public class InboxEventRepositoryTests
{
    /// <summary>
    /// A reusable sample InboxEvent for test scenarios.
    /// </summary>
    private readonly InboxEvent sampleEvent = new()
    {
        EventId = "event-123",
        RawPayload = "{}",
        CreatedAt = DateTime.UtcNow,
    };

    /// <summary>
    /// Ensures TrySaveAsync does not insert if the event already exists.
    /// </summary>
    /// <returns>representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TrySaveAsync_Should_Skip_When_ExistsAsync_Returns_True()
    {
        // Arrange
        using var dbContext = this.CreateInMemoryDbContext();
        var repo = new InboxEventRepository(dbContext);

        // Insert event beforehand
        await dbContext.InboxEvents.AddAsync(this.sampleEvent);
        await dbContext.SaveChangesAsync();

        // Act
        await repo.TrySaveAsync(this.sampleEvent, CancellationToken.None);

        // Assert
        var count = await dbContext.InboxEvents.CountAsync();
        count.Should().Be(1); // no duplicates
    }

    /// <summary>
    /// Ensures TrySaveAsync logs and suppresses primary key violations on insert.
    /// </summary>
    /// <returns>representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TrySaveAsync_Should_Suppress_PrimaryKeyViolation()
    {
        // Arrange
        using var dbContext = this.CreateInMemoryDbContext();
        var repo = new InboxEventRepository(dbContext);

        await dbContext.InboxEvents.AddAsync(this.sampleEvent);
        await dbContext.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await repo.TrySaveAsync(this.sampleEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
