// Domain/Repositories/IInboxEventLogRepository.cs
using System.Threading;
using System.Threading.Tasks;
using PaymentProcessor.Domain.Entities;

namespace PaymentProcessor.Domain.Repositories;

public interface IInboxEventLogRepository
{
    void Add(InboxEventLog log);
}
