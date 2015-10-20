namespace StatsDHelper.WebApi
{
    public class Constants
    {
        public static string StopwatchKey = "ExecutionTimeStopwatch";

        public class Configuration
        {
            public static string LatencyHeaderEnabled = "StatsD.WebApi.Response.LatencyHeader.Enabled";
        }
    }
}