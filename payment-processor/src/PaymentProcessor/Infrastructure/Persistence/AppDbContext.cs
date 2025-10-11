// <copyright file="AppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Events;
using PaymentProcessor.Infrastructure.Persistence.Auditing;
using PaymentProcessor.Infrastructure.Persistence.Entities.Payment;

namespace PaymentProcessor.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the payment processor.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    /// <summary>Inbox events (Inbox pattern state table).</summary>
    public virtual DbSet<InboxEvent> InboxEvents => Set<InboxEvent>();

    /// <summary>Processed payment records (normalized business data).</summary>
    public DbSet<PaymentEventRecord> PaymentEventRecords => Set<PaymentEventRecord>();

    /// <summary>Append-only audit trail for inbox event processing.</summary>
    public virtual DbSet<InboxEventLog> InboxEventLogs => Set<InboxEventLog>();

    /// <summary>
    /// Overrides SaveChanges to auto-populate audit fields on entities
    /// that implement <see cref="IAuditableEntity"/>.
    /// NOTE: InboxEventLog is append-only and should NOT implement IAuditableEntity.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
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

        // Store InboxEventStatus as string on the primary table
        var statusConverter = new EnumToStringConverter<InboxEventStatus>();

        // ========== InboxEvent ==========
        modelBuilder.Entity<InboxEvent>(e =>
        {
            e.ToTable("inbox_events");
            e.HasKey(x => x.EventId);

            e.Property(x => x.Status)
                .IsRequired()
                .HasConversion(statusConverter);

            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.UpdatedAt).IsRequired();

            // Optional: add targeted indexes by your query patterns
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.UpdatedAt);
        });

        // ========== PaymentEventRecord ==========
        modelBuilder.Entity<PaymentEventRecord>(e =>
        {
            e.ToTable("payment_event_records");
            e.HasKey(x => x.EventId);

            e.Property(x => x.Amount).IsRequired();
            e.Property(x => x.Currency).IsRequired();
            e.Property(x => x.Method).IsRequired();
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.EventAt).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.UpdatedAt).IsRequired();

            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.EventAt);
        });

        // ========== InboxEventLog (append-only audit) ==========
        // Matches the revised entity: only CreatedAt (UTC), no UpdatedAt,
        // string statuses, FK Restrict, and indexes for common queries.
        modelBuilder.Entity<InboxEventLog>(b =>
        {
            b.ToTable("inbox_event_logs");
            b.HasKey(x => x.Id);

            // Required fields
            b.Property(x => x.InboxEventId).IsRequired();
            b.Property(x => x.NewStatus).HasMaxLength(50).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            // Optional metadata
            b.Property(x => x.OldStatus).HasMaxLength(50);
            b.Property(x => x.EventType).HasMaxLength(100);
            b.Property(x => x.WorkerNode).HasMaxLength(100);
            b.Property(x => x.ErrorCode).HasMaxLength(100);
            b.Property(x => x.CorrelationId).HasMaxLength(100);
            b.Property(x => x.ErrorMessage).HasColumnType("text"); // long messages
            b.Property(x => x.AttemptNo);
            b.Property(x => x.IsSuccess);
            b.Property(x => x.DurationMs);

            // FK to inbox_events with Restrict to preserve audit rows
            b.HasOne(x => x.InboxEvent)
                .WithMany()
                .HasForeignKey(x => x.InboxEventId)
                .HasPrincipalKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Useful indexes for investigations/analytics
            b.HasIndex(x => x.InboxEventId);
            b.HasIndex(x => new { x.InboxEventId, x.CreatedAt });
            b.HasIndex(x => new { x.IsSuccess, x.CreatedAt });
            b.HasIndex(x => new { x.NewStatus, x.CreatedAt });

            // Prevent duplicate audit rows for the same attempt/status
            b.HasIndex(x => new { x.InboxEventId, x.AttemptNo, x.NewStatus })
             .IsUnique();
        });
    }
}
