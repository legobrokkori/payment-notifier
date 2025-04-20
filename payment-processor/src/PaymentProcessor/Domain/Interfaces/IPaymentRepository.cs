// <copyright file="IPaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Domain.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    using PaymentProcessor.Domain.Entities;

    /// <summary>
    /// Interface for persisting <see cref="PaymentEvent"/> entities to the database.
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Saves a payment event to the underlying data store.
        /// </summary>
        /// <param name="payment">The payment event to save.</param>
        /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveAsync(PaymentEvent payment, CancellationToken cancellationToken);
    }
}
