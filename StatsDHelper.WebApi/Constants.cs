namespace StatsDHelper.WebApi
{
    internal class Constants
    {
        internal static string StopwatchKey = "ExecutionTimeStopwatch";

        internal class Configuration
        {
            internal static string LatencyHeaderEnabled = "StatsD.WebApi.Response.LatencyHeader.Enabled";
        }
    }
}