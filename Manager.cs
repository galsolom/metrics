using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace ghmonitor {
    public class Manager {
        public readonly List<string> monitoredServices;
        private readonly string _baseUrl;

        public Manager () {
            _baseUrl = getBaseUrl ();
            monitoredServices = getMonitoredServices ();
        }

        List<string> getMonitoredServices () =>
            Environment.GetEnvironmentVariable ("GH_MONITORED").Split (",").ToList ();
            
        string getBaseUrl () => Environment.GetEnvironmentVariable ("GH_STATUSURL");

        public async Task<Dictionary<string, int>> report () {
            var metrics = new Dictionary<string, int> ();
            var stats = await getComponentStatus ();
            var _results = stats.Keys.Where (a => monitoredServices.Contains (a));
            foreach (var result in _results) {
                var operational = stats[result] == "operational";
                if (operational) {
                    Logger.log (LogLevel.Information, $"{result} is currently {stats[result]}");
                } else {
                    Logger.log (LogLevel.Error, $"{result} is currently {stats[result]}");
                }

                metrics.Add (utils.formatName (result), stats[result] == "operational" ? 1 : 0);
            }
            return metrics;
        }

        public async Task testConnectivity () {
            try {
                Logger.log (LogLevel.Information, $"TEST::  connectivity to {_baseUrl}");
                var _components = await get (_baseUrl);
                var statuses = await getComponentStatus ();
                Logger.log (LogLevel.Information,
                    "Available Github Components to monitor:");
                foreach (var a in statuses.Keys) {
                    Console.WriteLine (a);
                }
                Logger.log (LogLevel.Information, "Currently monitoring the following:");
                foreach (var service in monitoredServices) {
                    Console.WriteLine (service);
                }

            } catch (System.Exception e) {
                Logger.log (LogLevel.Error,
                    $"ERROR:: failed to connect {_baseUrl} {e.Message}");
                throw new Exception ($"failed to connect {_baseUrl}");
            }
        }
        async Task<Dictionary<string, string>> getComponentStatus () {
            var _jobject = await get (_baseUrl);
            var _dict = _jobject["components"]
                .ToDictionary (p => p["name"], p => p["status"])
                .Where (kv => !kv.Key.Contains ("githubstatus")).ToDictionary (p => (string) p.Key, p => (string) p.Value);

            return _dict;
        }

        private async Task<JObject> get (string _url) {
            using (HttpClient client = new HttpClient ()) {
                client.DefaultRequestHeaders.Accept.Add (
                    new MediaTypeWithQualityHeaderValue ("application/json"));
                using (HttpResponseMessage response = await client.GetAsync (_url)) {
                    string responseBody = await response.Content.ReadAsStringAsync ();
                    return JObject.Parse (responseBody);
                }
            }
        }
    }
}