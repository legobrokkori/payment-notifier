using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class PaymentWorker : BackgroundService
{
    private readonly ILogger<PaymentWorker> _logger;

    public PaymentWorker(ILogger<PaymentWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PaymentWorker running");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Waiting for messages...");
            await Task.Delay(3000, stoppingToken);
        }

        _logger.LogInformation("PaymentWorker stopped");
    }
}
