using System;
using FluentAssertions;
using PaymentProcessor.Domain.Entities;
using Xunit;

namespace PaymentProcessor.Tests.Domain.Entities;

public class PaymentEventTests
{
    [Fact]
    public void Constructor_Should_Throw_When_Required_Field_Is_Missing()
    {
        Action act = () => new PaymentEvent("", 1000, "USD", "card", "paid", "2024-04-01T10:00:00Z");
        act.Should().Throw<ArgumentException>().WithMessage("*required*");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Invalid_Date_Format()
    {
        Action act = () => new PaymentEvent("evt_001", 1000, "USD", "card", "paid", "not-a-date");
        act.Should().Throw<ArgumentException>().WithMessage("*eventAt format*");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Invalid_Status()
    {
        Action act = () => new PaymentEvent("evt_001", 1000, "USD", "card", "unknown", "2024-04-01T10:00:00Z");
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid status*");
    }

    [Fact]
    public void Constructor_Should_Succeed_With_Valid_Input()
    {
        var evt = new PaymentEvent("evt_001", 1000, "USD", "card", "paid", "2024-04-01T10:00:00Z");
        evt.Id.Should().Be("evt_001");
        evt.Amount.Should().Be(1000);
        evt.Currency.Should().Be("USD");
        evt.Method.Should().Be("card");
        evt.Status.Should().Be("paid");
        evt.EventAt.Should().Be(DateTime.Parse("2024-04-01T10:00:00Z"));
    }

    [Fact]
    public void Constructor_Should_Convert_EventAt_To_Utc()
    {
        var input = "2024-04-01T19:00:00+09:00";
        var evt = new PaymentEvent("evt_001", 1200, "JPY", "card", "paid", input);

        evt.EventAt.Should().Be(new DateTime(2024, 4, 1, 10, 0, 0, DateTimeKind.Utc));
    }
}
