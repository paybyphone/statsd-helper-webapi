using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Filters
{
    public static class HttpActionExecutedContextExtensions
    {
        private static readonly IStatsDHelper StatsDHelper = global::StatsDHelper.StatsDHelper.Instance;
        private static readonly IDictionary<string, Func<HttpActionExecutedContext, object>> TemplateRegistry = new Dictionary<string, Func<HttpActionExecutedContext, object>>
        {
            { "action", context => context.ActionContext.ActionDescriptor.ActionName },
            { "controller", context => context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName }
        };

        public static void InstrumentResponse(this HttpActionExecutedContext httpActionExecutedContext, string template = "{action}")
        {
            var requestStopwatch = httpActionExecutedContext.Request.Properties[Constants.StopwatchKey] as Stopwatch;

            if (httpActionExecutedContext.Response != null)
            {
                var metricName = template;

                foreach (string templatedValue in TemplateRegistry.Keys)
                {
                    var resolver = TemplateRegistry[templatedValue];
                    var value = resolver(httpActionExecutedContext);

                    metricName = metricName.Replace("{" + templatedValue + "}", value.ToString().ToLowerInvariant());
                }

                StatsDHelper.LogCount($"{metricName}.{(int) httpActionExecutedContext.Response.StatusCode}");

                if (requestStopwatch != null)
                {   
                    requestStopwatch.Stop();
                    StatsDHelper.LogTiming($"{metricName}.latency", (long)requestStopwatch.Elapsed.TotalMilliseconds);
                    var response = httpActionExecutedContext.Response;
                    response.Headers.Add("X-ExecutionTime", requestStopwatch.Elapsed.ToString());
                }
            }
        }
    }
}