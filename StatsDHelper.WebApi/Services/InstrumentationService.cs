using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Services
{
    internal class InstrumentationService : IInstrumentationService
    {
        private readonly IAppSettings _appSettings;
        private readonly IStatsDHelper _statsDHelper = StatsDHelper.Instance;

        private readonly IDictionary<string, Func<HttpActionExecutedContext, object>> _templateRegistry = new Dictionary<string, Func<HttpActionExecutedContext, object>>
        {
            {"action", context => context.ActionContext.ActionDescriptor.ActionName},
            {"controller", context => context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName}
        };

        public InstrumentationService()
        {
            _appSettings = new AppSettings();
        }

        internal InstrumentationService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void InstrumentResponse(HttpActionExecutedContext httpActionExecutedContext, string template = "{action}")
        {
            if (httpActionExecutedContext.Response != null)
            {
                var metricName = template;

                foreach (string templatedValue in _templateRegistry.Keys)
                {
                    var resolver = _templateRegistry[templatedValue];
                    var value = resolver(httpActionExecutedContext);

                    metricName = metricName.Replace("{" + templatedValue + "}", value.ToString().ToLowerInvariant());
                }

                _statsDHelper.LogCount($"{metricName}.{(int) httpActionExecutedContext.Response.StatusCode}");

                var requestStopwatch = httpActionExecutedContext.Request.Properties[Constants.StopwatchKey] as Stopwatch;

                if (requestStopwatch != null)
                {
                    requestStopwatch.Stop();
                    _statsDHelper.LogTiming($"{metricName}.latency", (long) requestStopwatch.Elapsed.TotalMilliseconds);

                    if (_appSettings.GetBoolean(Constants.Configuration.LatencyHeaderEnabled))
                    {
                        var response = httpActionExecutedContext.Response;
                        response.Headers.Add("X-ExecutionTime", requestStopwatch.Elapsed.ToString());
                    }
                }
            }
        }
    }
}