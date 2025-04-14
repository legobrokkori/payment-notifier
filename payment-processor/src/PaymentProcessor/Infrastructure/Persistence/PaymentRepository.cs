using System.Threading;
using System.Threading.Tasks;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Interfaces;

namespace PaymentProcessor.Infrastructure.Persistence
{
    public class PaymentRepository : IPaymentRepository
    {
        public Task SaveAsync(PaymentEvent payment, CancellationToken cancellationToken)
        {
            // TODO: Implement EF-based persistence later
            return Task.CompletedTask;
        }
    }
}