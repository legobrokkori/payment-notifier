using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Repositories;

namespace PaymentProcessor.Infrastructure.Persistence.Repositories;

public sealed class InboxEventLogRepository : IInboxEventLogRepository
{
    private readonly AppDbContext db;

    public InboxEventLogRepository(AppDbContext db)
    {
        this.db = db;
    }

    public void Add(InboxEventLog log)
    {
        db.InboxEventLogs.Add(log);
    }
}
