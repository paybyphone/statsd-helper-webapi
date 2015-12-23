using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Services
{
    internal class InstrumentationService : IInstrumentationService
    {
        private readonly IAppSettings _appSettings;
        private readonly IStatsDHelper _statsDHelper = StatsDHelper.Instance;

        private readonly IDictionary<string, Func<HttpActionDescriptor, object>> _templateRegistry = new Dictionary<string, Func<HttpActionDescriptor, object>>
        {
            {"action", actionDescriptor => actionDescriptor.ActionName},
            {"controller", actionDescriptor => actionDescriptor.ControllerDescriptor.ControllerName}
        };

        public InstrumentationService()
        {
            _appSettings = new AppSettings();
        }

        internal InstrumentationService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void TimeRequest(HttpRequestMessage request)
        {
            var requestStopwatch = new Stopwatch();
            request.Properties.Add(Constants.StopwatchKey, requestStopwatch);
            requestStopwatch.Start();
        }

        public void InstrumentResponse(HttpActionExecutedContext httpActionExecutedContext, string template = "{action}")
        {
            InstrumentResponse(httpActionExecutedContext.Response, template);
        }

        public void InstrumentResponse(HttpResponseMessage response, string template = "{action}")
        {
            if (response != null && response.RequestMessage != null)
            {
                var actionDescriptor = response.RequestMessage.GetActionDescriptor();
                if (actionDescriptor != null)
                {
                    var metricName = template;

                    foreach (string templatedValue in _templateRegistry.Keys)
                    {
                        var resolver = _templateRegistry[templatedValue];
                        var value = resolver(actionDescriptor);

                        metricName = metricName.Replace("{" + templatedValue + "}", value.ToString().ToLowerInvariant());
                    }

                    _statsDHelper.LogCount(string.Format("{0}.{1}", metricName, (int) response.StatusCode));

                    var requestStopwatch = response.RequestMessage.Properties[Constants.StopwatchKey] as Stopwatch;

                    if (requestStopwatch != null)
                    {
                        requestStopwatch.Stop();
                        _statsDHelper.LogTiming(string.Format("{0}.latency", metricName),
                            (long) requestStopwatch.Elapsed.TotalMilliseconds);

                        if (_appSettings.GetBoolean(Constants.Configuration.LatencyHeaderEnabled))
                        {
                            response.Headers.Add("X-ExecutionTime",
                                string.Format("{0}ms", Math.Round(requestStopwatch.Elapsed.TotalMilliseconds)));
                        }
                    }
                }
            }
        }
    }
}