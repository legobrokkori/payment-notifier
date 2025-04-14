using System;
using FluentAssertions;
using PaymentProcessor.Domain.Entities;
using Xunit;

namespace PaymentProcessor.Tests.Domain.Entities;

public class PaymentEventTests
{
    [Fact]
    public void Create_ValidEvent_ShouldSucceed()
    {
        var evt = new PaymentEvent("evt_001", 1000, "USD", "card", "paid", DateTime.UtcNow);

        evt.Should().NotBeNull();
        evt.Id.Should().Be("evt_001");
    }

    [Fact]
    public void Create_InvalidStatus_ShouldThrow()
    {
        Action act = () => new PaymentEvent("evt_002", 1000, "USD", "card", "bad-status", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*invalid*");
    }
}
