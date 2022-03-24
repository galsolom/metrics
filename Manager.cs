using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace ghmonitor {
    public class Manager {
        private readonly List<string> monitoredServices;
        private readonly string _baseUrl;

        public Manager () {
            _baseUrl = getBaseUrl ();
            monitoredServices = getMonitoredServices ();
        }
        public async Task report () {
            var stats = await getComponentStatus ();
            var _results = stats.Keys.Where (a => monitoredServices.Contains(a));
            if (_results.Count () > 0) {
                foreach (var result in _results) {
                    if (stats[result] != "operational") {
                        Logger.log (LogLevel.Information, $"{result} is currently {stats[result]}");
                    }
                }
            }
        }

        List<string> getMonitoredServices () =>
            Environment.GetEnvironmentVariable ("GH_MONITORED").Split (",").ToList ();
        string getBaseUrl () => Environment.GetEnvironmentVariable ("GH_STATUSURL");

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
            var _dict = new Dictionary<string, string> ();
            var componenets = _jobject["components"];
            var comps = JArray.FromObject (componenets);
            foreach (var component in comps) {
                // can be changed to ignore list
                if (!component["name"].ToString ().Contains ("www.githubstatus.com"))
                    _dict.Add (component["name"].ToString (), component["status"].ToString ());
            }
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