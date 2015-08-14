using System.Text;
using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Filters
{
    public static class HttpActionExecutedContextExtensions
    {
        private static readonly IStatsDHelper StatsDHelper = global::StatsDHelper.StatsDHelper.Instance;

        public static void InstrumentResponse(this HttpActionExecutedContext httpActionExecutedContext, bool includeActionName = true, bool includeControllerName = false)
        {
            if (httpActionExecutedContext.Response != null)
            {
                var metricNameBuilder = new StringBuilder();

                if (includeControllerName)
                {
                    var controllerName = httpActionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    metricNameBuilder.Append(controllerName);
                    metricNameBuilder.Append(".");
                }

                if (includeActionName)
                {
                    var actionName = httpActionExecutedContext.ActionContext.ActionDescriptor.ActionName;
                    metricNameBuilder.Append(actionName);
                    metricNameBuilder.Append(".");
                }

                StatsDHelper.LogCount(string.Format("{0}{1}", metricNameBuilder, (int)httpActionExecutedContext.Response.StatusCode));
            }
        }
    }
}