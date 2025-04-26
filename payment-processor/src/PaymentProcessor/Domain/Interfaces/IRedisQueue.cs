// <copyright file="IRedisQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Domain.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a contract for interacting with a Redis-based queue.
    /// </summary>
    public interface IRedisQueue
    {
        /// <summary>
        /// Pops the next message from the Redis queue.
        /// </summary>
        /// <param name="queue">The name of the Redis queue.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the raw message string, or null if no message is available.
        /// </returns>
        Task<string?> PopAsync(string queue, CancellationToken cancellationToken);
    }
}
