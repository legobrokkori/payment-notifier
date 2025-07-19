// <copyright file="InboxEventStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Domain.Events
{
    /// <summary>
    /// Represents the processing status of an inbox event.
    /// Used to control event lifecycle in the Inbox Pattern.
    /// </summary>
    public enum InboxEventStatus
    {
        /// <summary>
        /// The event has been saved but not yet processed.
        /// </summary>
        Pending,

        /// <summary>
        /// The event is currently being processed.
        /// </summary>
        Processing,

        /// <summary>
        /// The event was processed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The event failed to process.
        /// </summary>
        Failed,
    }
}
