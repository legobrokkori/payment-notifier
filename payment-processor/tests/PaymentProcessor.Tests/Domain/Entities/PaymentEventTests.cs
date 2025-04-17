// <copyright file="PaymentEventTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Tests.Domain.Entities
{
    using System;

    using FluentAssertions;

    using PaymentProcessor.Domain.Entities;

    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="PaymentEvent"/> entity.
    /// </summary>
    public class PaymentEventTests
    {
        /// <summary>
        /// Should throw an exception when required fields are missing.
        /// </summary>
        [Fact]
        public void Constructor_Should_Throw_When_Fields_Are_Missing()
        {
            Action act = () => new PaymentEvent(string.Empty, 0, string.Empty, string.Empty, string.Empty, "2024-04-01T00:00:00Z");

            act.Should().Throw<ArgumentException>().WithMessage("All fields are required.");
        }

        /// <summary>
        /// Should throw an exception when the status is invalid.
        /// </summary>
        [Fact]
        public void Constructor_Should_Throw_When_Status_Is_Invalid()
        {
            Action act = () => new PaymentEvent("abc", 1000, "USD", "card", "invalid", "2024-04-01T00:00:00Z");

            act.Should().Throw<ArgumentException>().WithMessage("Invalid status.");
        }

        /// <summary>
        /// Should throw an exception when eventAt format is invalid.
        /// </summary>
        [Fact]
        public void Constructor_Should_Throw_When_EventAt_Is_Invalid_Format()
        {
            Action act = () => new PaymentEvent("abc", 1000, "USD", "card", "paid", "invalid-timestamp");

            act.Should().Throw<ArgumentException>().WithMessage("Invalid eventAt format.");
        }

        /// <summary>
        /// Should create the event successfully when all inputs are valid.
        /// </summary>
        [Fact]
        public void Constructor_Should_Succeed_With_Valid_Input()
        {
            var evt = new PaymentEvent("abc", 1000, "USD", "card", "paid", "2024-04-01T10:00:00Z");

            evt.Id.Should().Be("abc");
            evt.Amount.Should().Be(1000);
            evt.Currency.Should().Be("USD");
            evt.Method.Should().Be("card");
            evt.Status.Should().Be("paid");

            evt.EventAt.UtcDateTime.Should().Be(DateTimeOffset.Parse("2024-04-01T10:00:00Z").UtcDateTime);
        }

        /// <summary>
        /// Should correctly convert a non-UTC eventAt string (with offset) to UTC internally.
        /// </summary>
        [Fact]
        public void Constructor_Should_Convert_NonUtc_EventAt_To_Utc()
        {
            var evt = new PaymentEvent("abc", 1000, "USD", "card", "paid", "2024-04-01T19:00:00+09:00");

            evt.EventAt.UtcDateTime.Should().Be(DateTimeOffset.Parse("2024-04-01T10:00:00Z").UtcDateTime);
        }
    }
}
