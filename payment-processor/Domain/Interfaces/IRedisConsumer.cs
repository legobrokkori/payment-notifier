namespace PaymentProcessor.Domain.Interfaces;

public interface IRedisConsumer
{
    Task ConsumeAsync(CancellationToken cancellationToken);
}