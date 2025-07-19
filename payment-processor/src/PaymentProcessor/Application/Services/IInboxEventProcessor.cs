namespace PaymentProcessor.Application.Services;

public interface IInboxEventProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);
}
