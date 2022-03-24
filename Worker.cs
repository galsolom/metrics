using Newtonsoft.Json.Linq;

namespace ghmonitor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Manager _manager;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _manager = new Manager();
        Logger.log(LogLevel.Information,"START:: GitHub service monitor");

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _manager.testConnectivity();
        while (!stoppingToken.IsCancellationRequested)
        {
            await _manager.report();
            await Task.Delay(10000, stoppingToken);
        }
    }
}
