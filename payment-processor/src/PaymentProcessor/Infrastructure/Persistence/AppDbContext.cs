using Microsoft.EntityFrameworkCore;
using PaymentProcessor.Domain.Entities;

namespace PaymentProcessor.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();
    }
}
