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
            // Load configuration from appsettings.Development.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Ensure this points to your working directory
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Extract the connection string from configuration
            var connectionString = configuration.GetSection("Database")["ConnectionString"];

            // Configure EF Core with PostgreSQL or other provider
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Return a new instance of your AppDbContext
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
