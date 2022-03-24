
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ghmonitor
{
    public static class Logger
    {
        public static void log(LogLevel level, string msg)
        {
            var logdata = new Dictionary<string, object>
            {
                {"Level", level.ToString()},
                {"Message", msg},
                {"Date",DateTime.Now}
            };
            var jsonlog = JsonConvert.SerializeObject(logdata);
            Console.WriteLine(jsonlog);
        }
    }
}