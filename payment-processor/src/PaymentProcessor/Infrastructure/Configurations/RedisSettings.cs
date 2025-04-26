// <copyright file="RedisSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Configurations
{
    /// <summary>
    /// Represents the configuration settings for connecting to a Redis instance.
    /// </summary>
    public class RedisSettings
    {
        /// <summary>
        /// Gets or sets the hostname or IP address of the Redis server.
        /// </summary>
        required public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port number used to connect to the Redis server.
        /// </summary>
        required public int Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the Redis queue to consume messages from.
        /// </summary>
        required public string Queue { get; set; }

        /// <summary>
        /// Gets or sets the Redis Stream consumer group name.
        /// This group is used by StreamReadGroup and StreamAcknowledge
        /// to manage message consumption and delivery tracking.
        /// </summary>
        required public string Group { get; set; } = "payment-group";

        /// <summary>
        /// Gets or sets the maximum number of entries to read from the stream at once.
        /// </summary>
        required public int ReadCount { get; set; } = 1; // Default to 1 for safety
    }
}
