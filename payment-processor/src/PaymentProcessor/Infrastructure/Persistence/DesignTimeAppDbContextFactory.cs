namespace PaymentProcessor.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    using PaymentProcessor.Infrastructure.Persistence;

    /// <summary>
    /// Factory class used by EF Core CLI to instantiate <see cref="AppDbContext"/> at design time.
    /// This avoids relying on runtime services like Redis or other DI-managed dependencies.
    /// </summary>
    public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="AppDbContext"/> using configuration from appsettings.json.
        /// </summary>
        /// <param name="args">Arguments passed by the EF Core tools.</param>
        /// <returns>A new configured instance of <see cref="AppDbContext"/>.</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Environment variable 'CONNECTIONSTRING' is not set.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
