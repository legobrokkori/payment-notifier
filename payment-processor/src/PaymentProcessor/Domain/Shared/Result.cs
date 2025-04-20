// <copyright file="Result.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Domain.Shared
{
    /// <summary>
    /// Represents the result of an operation that may succeed or fail.
    /// This is a simple implementation of the Result pattern commonly used in domain-driven design.
    /// </summary>
    /// <typeparam name="T">The type of the successful result value.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the error message, if the operation failed.
        /// This is null when <see cref="IsSuccess"/> is true.
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Gets the value returned by a successful operation.
        /// This is null when <see cref="IsSuccess"/> is false.
        /// </summary>
        public T? Value { get; }

        private Result(T value)
        {
            this.IsSuccess = true;
            this.Value = value;
            this.Error = null;
        }

        private Result(string error)
        {
            this.IsSuccess = false;
            this.Error = error;
            this.Value = default;
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value of the result.</param>
        /// <returns>A <see cref="Result{T}"/> representing success.</returns>
        public static Result<T> Success(T value) => new Result<T>(value);

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="error">The error message describing the failure.</param>
        /// <returns>A <see cref="Result{T}"/> representing failure.</returns>
        public static Result<T> Failure(string error) => new Result<T>(error);
    }
}
