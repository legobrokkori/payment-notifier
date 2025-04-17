// <copyright file="DatabaseSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Configurations
{
    /// <summary>
    /// Represents the database configuration settings, including the connection string.
    /// These settings are typically loaded from appsettings.json or environment variables.
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// Gets or sets the connection string for the PostgreSQL database.
        /// </summary>
        public string ConnectionString { get; set; } = default!;
    }
}
