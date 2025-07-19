using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PaymentProcessor.Infrastructure.Persistence;

namespace PaymentProcessor.Infrastructure.Factories
{
    /// <summary>
    /// Used by EF Core CLI tools (e.g. dotnet ef) to create DbContext at design-time.
    /// </summary>
    public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
    {
        public PaymentDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Environment variable 'CONNECTIONSTRING' is not set.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new PaymentDbContext(optionsBuilder.Options);
        }
    }
}
