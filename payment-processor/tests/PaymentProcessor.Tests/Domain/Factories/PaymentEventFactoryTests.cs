// <copyright file="PaymentEventFactoryTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PaymentProcessor.Tests.Domain.Factories
{
    using System;

    using FluentAssertions;

    using PaymentProcessor.Domain.Factories;

    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="PaymentEventFactory"/>.
    /// </summary>
    public class PaymentEventFactoryTests
    {
        /// <summary>
        /// Should create a valid PaymentEvent when all input parameters are valid.
        /// </summary>
        [Fact]
        public void TryCreate_Should_Succeed_With_Valid_Parameters()
        {
            var result = PaymentEventFactory.TryCreate("abc", 1000, "USD", "card", "paid", "2024-04-01T10:00:00Z");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be("abc");
            result.Value!.EventAt.UtcDateTime.Should().Be(DateTimeOffset.Parse("2024-04-01T10:00:00Z").UtcDateTime);
        }

        /// <summary>
        /// Should fail to create a PaymentEvent when the status value is invalid.
        /// </summary>
        [Fact]
        public void TryCreate_Should_Fail_When_Status_Is_Invalid()
        {
            var result = PaymentEventFactory.TryCreate("abc", 1000, "USD", "card", "invalid", "2024-04-01T10:00:00Z");

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Invalid status.");
        }

        /// <summary>
        /// Should fail to create a PaymentEvent when eventAt is not a valid ISO 8601 timestamp.
        /// </summary>
        [Fact]
        public void TryCreate_Should_Fail_When_EventAt_Is_Invalid_Format()
        {
            var result = PaymentEventFactory.TryCreate("abc", 1000, "USD", "card", "paid", "not-a-date");

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Invalid eventAt format.");
        }

        /// <summary>
        /// Should fail to create a PaymentEvent when required fields are missing or invalid.
        /// </summary>
        [Fact]
        public void TryCreate_Should_Fail_When_Required_Fields_Are_Missing()
        {
            var result = PaymentEventFactory.TryCreate("", 0, "", "", "", "");

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("All fields are required.");
        }

        /// <summary>
        /// Should correctly convert a non-UTC eventAt value with timezone offset to UTC.
        /// </summary>
        [Fact]
        public void TryCreate_Should_Convert_EventAt_With_Offset_To_Utc()
        {
            var result = PaymentEventFactory.TryCreate("abc", 1000, "USD", "card", "paid", "2024-04-01T19:00:00+09:00");

            result.IsSuccess.Should().BeTrue();
            result.Value!.EventAt.UtcDateTime.Should().Be(DateTimeOffset.Parse("2024-04-01T10:00:00Z").UtcDateTime);
        }
    }
}
