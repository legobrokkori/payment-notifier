// <copyright file="AppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;

    using PaymentProcessor.Infrastructure.Persistence.Auditing;
    using PaymentProcessor.Infrastructure.Persistence.Entities.Inbox;
    using PaymentProcessor.Infrastructure.Persistence.Entities.Payment;

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
        /// Gets the Inbox Events table (used for implementing the Inbox Pattern).
        /// </summary>
        public virtual DbSet<InboxEvent> InboxEvents => this.Set<InboxEvent>();

        /// <summary>
        /// Gets the Payment Event Records table (persistent storage for processed events).
        /// </summary>
        public DbSet<PaymentEventRecord> PaymentEventRecords => this.Set<PaymentEventRecord>();

        /// <summary>
        /// Saves all changes made in this context to the database,
        /// and automatically updates audit fields (CreatedAt, UpdatedAt) for entities
        /// that implement <see cref="IAuditableEntity"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTimeOffset.UtcNow;

            foreach (var entry in this.ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure InboxEvent entity
            modelBuilder.Entity<InboxEvent>(entity =>
            {
                entity.ToTable("inbox_events");
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.RetryCount).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.ProcessingStartedAt).IsRequired(false);
            });

            // Configure PaymentEventRecord entity
            modelBuilder.Entity<PaymentEventRecord>(entity =>
            {
                entity.ToTable("payment_event_records");
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.Amount).IsRequired();
                entity.Property(e => e.Currency).IsRequired();
                entity.Property(e => e.Method).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.EventAt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });
        }
    }
}
