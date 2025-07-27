// <copyright file="RedisValueExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Infrastructure.Redis
{
    using StackExchange.Redis;

    /// <summary>
    /// Extension method to safely extract byte array from RedisValue.
    /// </summary>
    internal static class RedisValueExtensions
    {
        public static bool TryGetValue(this RedisValue value, out byte[] ? bytes)
        {
            try
            {
                bytes = (byte[])value;
                return true;
            }
            catch
            {
                bytes = null;
                return false;
            }
        }
    }
}
