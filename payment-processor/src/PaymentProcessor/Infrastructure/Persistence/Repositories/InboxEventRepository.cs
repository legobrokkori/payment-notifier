namespace PaymentProcessor.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;

using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Events;
using PaymentProcessor.Domain.Repositories;

public class InboxEventRepository : IInboxEventRepository
{
    private readonly AppDbContext dbContext;

    public InboxEventRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task SaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken)
    {
        this.dbContext.InboxEvents.Add(inboxEvent);
        await this.dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task TrySaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken)
    {
        var exists = await this.dbContext.InboxEvents
            .AnyAsync(x => x.EventId == inboxEvent.EventId, cancellationToken);

        if (!exists)
        {
            await this.SaveAsync(inboxEvent, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<InboxEvent>> DequeuePendingAsync(int limit, CancellationToken cancellationToken)
    {
        return await this.dbContext.InboxEvents
            .Where(e => e.Status == InboxEventStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await this.dbContext.SaveChangesAsync(cancellationToken);
    }
}
