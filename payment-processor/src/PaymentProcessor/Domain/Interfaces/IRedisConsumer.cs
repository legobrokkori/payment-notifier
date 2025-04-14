using System.Threading;
using System.Threading.Tasks;
using PaymentProcessor.Domain.Entities;

namespace PaymentProcessor.Domain.Interfaces
{
    public interface IRedisConsumer
    {
        Task<PaymentEvent?> DequeueAsync(CancellationToken cancellationToken);
    }
}
