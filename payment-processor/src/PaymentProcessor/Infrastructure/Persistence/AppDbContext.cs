// <copyright file="AppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;

    using PaymentProcessor.Domain.Entities;

    /// <summary>
    /// Represents the Entity Framework Core database context for the payment processor.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/> of payment events.
        /// </summary>
        public DbSet<PaymentEvent> PaymentEvents => this.Set<PaymentEvent>();
    }
}
