// <copyright file="PaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;

    using PaymentProcessor.Domain.Entities;
    using PaymentProcessor.Domain.Interfaces;

    /// <summary>
    /// Provides persistence operations for <see cref="PaymentEvent"/> entities using Entity Framework.
    /// </summary>
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRepository"/> class.
        /// </summary>
        /// <param name="db">The application's database context.</param>
        public PaymentRepository(AppDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Persists the given <see cref="PaymentEvent"/> to the database.
        /// </summary>
        /// <param name="paymentEvent">The payment event to be saved.</param>
        /// <param name="cancellationToken">A cancellation token for the async operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
        {
            this.db.PaymentEvents.Add(paymentEvent);
            await this.db.SaveChangesAsync(cancellationToken);
        }
    }
}
