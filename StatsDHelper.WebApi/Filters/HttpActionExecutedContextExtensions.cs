using System;
using System.Collections.Generic;
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
            if (httpActionExecutedContext.Response != null)
            {
                var metricName = template;

                foreach (string templatedValue in TemplateRegistry.Keys)
                {
                    var resolver = TemplateRegistry[templatedValue];
                    var value = resolver(httpActionExecutedContext);

                    metricName = metricName.Replace("{" + templatedValue + "}", value.ToString().ToLowerInvariant());
                }

                StatsDHelper.LogCount(string.Format("{0}.{1}", metricName, (int)httpActionExecutedContext.Response.StatusCode));
            }
        }
    }
}