namespace PaymentProcessor.Application.Mappers;

using PaymentProcessor.Domain.Entities;
using PaymentProcessor.Domain.Factories;
using PaymentProcessor.Domain.Shared; // ← Result<T> 用

using DomainPaymentEvent = PaymentProcessor.Domain.Entities.PaymentEvent;
using ProtoPaymentEvent = Payment.PaymentEvent;

public static class PaymentEventMapper
{
    public static Result<DomainPaymentEvent> FromProto(ProtoPaymentEvent proto)
    {
        return PaymentEventFactory.TryCreate(
            id: proto.Id,
            amount: proto.Amount,
            currency: proto.Currency,
            method: proto.Method,
            status: proto.Status,
            eventAt: proto.OccurredAt);
    }
}
