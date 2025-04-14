using PaymentProcessor.Domain.Entities;

namespace PaymentProcessor.Domain.Interfaces;

public interface IPaymentRepository
{
    Task SaveAsync(PaymentEvent payment, CancellationToken cancellationToken);
}