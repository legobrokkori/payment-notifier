using Microsoft.EntityFrameworkCore;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Interfaces;

namespace PaymentProcessor.Infrastructure.Persistence
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _db;

        public PaymentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
        {
            // Insert the event
            _db.PaymentEvents.Add(paymentEvent);
            await _db.SaveChangesAsync(cancellationToken);
        }

    }
}