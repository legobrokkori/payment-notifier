using System;
using PaymentProcessor.Domain.Entities;

namespace PaymentProcessor.Domain.Repositories;

public interface IInboxEventRepository
{
    Task<IReadOnlyList<InboxEvent>> DequeuePendingAsync(int limit, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);

    Task SaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken);

    Task TrySaveAsync(InboxEvent inboxEvent, CancellationToken cancellationToken);
}
