using Newtonsoft.Json.Linq;
using Prometheus.Client;

namespace ghmonitor;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private readonly Manager _manager;
    private readonly Dictionary<string, IGauge> _gauges;

    public Worker (ILogger<Worker> logger, IConfiguration config, IMetricFactory metricFactory) {
        Logger.log (LogLevel.Information, "START:: GitHub service monitor");
        _logger = logger;
        _manager = new Manager ();
        var names = utils.formatNames (_manager.monitoredServices);
        _gauges = utils.createGauges (metricFactory, names);

    }

    protected override async Task ExecuteAsync (CancellationToken stoppingToken) {
        await _manager.testConnectivity ();
        while (!stoppingToken.IsCancellationRequested) {
            var stats = await _manager.report ();
            foreach (var _gauge in _gauges.Keys) {
                _gauges[_gauge].Set (stats[_gauge]);
            }

            await Task.Delay (5000, stoppingToken);
        }
    }
}