using ghmonitor;
using Prometheus.Client.Collectors;
using Prometheus.Client.DependencyInjection;
using Prometheus.Client.MetricServer;

IHost host = Host.CreateDefaultBuilder (args)
    .ConfigureServices (services => {
        services.AddMetricFactory ();
        services.AddSingleton<IMetricServer> (sp => new MetricServer (
            new MetricServerOptions {
                Host = "0.0.0.0",
                    Port = 8083,
                    CollectorRegistryInstance = sp.GetRequiredService<ICollectorRegistry> (),
                    UseDefaultCollectors = false
            }));
        services.AddHostedService<Worker> ();

    })
    .Build ();
var metricServer = host.Services.GetRequiredService<IMetricServer> ();

try {
    metricServer.Start ();
    await host.RunAsync ();
} catch (Exception ex) {
    Console.WriteLine ("Host Terminated Unexpectedly");
} finally {
    metricServer.Stop ();
}

await host.RunAsync ();