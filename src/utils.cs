using Prometheus.Client;
public static class utils {

    public static List<string> formatNames (List<String> list) => list.Select (
        item => "github_" + item.Replace (" ", "_").ToLower ()
    ).ToList ();
    
    public static string formatName (string str) => "github_" + str.Replace (" ", "_").ToLower ();

    public static Dictionary<string, IGauge> createGauges (IMetricFactory metricFactory, List<string> names) {
        var _dict = new Dictionary<string, IGauge> ();
        foreach (var name in names) {
            var _gauge = metricFactory.CreateGauge (name, $"is {name} is up");
            _dict.Add (name, _gauge);
        }
        return _dict;
    }

}