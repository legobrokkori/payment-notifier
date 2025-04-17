// <copyright file="IRedisConsumer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Domain.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    using PaymentProcessor.Domain.Entities;

    /// <summary>
    /// Defines a contract for consuming payment events from a Redis queue.
    /// </summary>
    public interface IRedisConsumer
    {
        /// <summary>
        /// Dequeues the next <see cref="PaymentEvent"/> from Redis.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The dequeued payment event, or null if none available.</returns>
        Task<PaymentEvent?> DequeueAsync(CancellationToken cancellationToken);
    }
}
