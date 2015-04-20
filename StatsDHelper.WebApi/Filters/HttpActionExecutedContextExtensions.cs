using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Filters
{
    public static class HttpActionExecutedContextExtensions
    {
        private static readonly IStatsDHelper StatsDHelper = global::StatsDHelper.StatsDHelper.Instance;

        public static void InstrumentResponse(this HttpActionExecutedContext httpActionExecutedContext)
        {
            if (httpActionExecutedContext.Response != null)
            {
                var actionName = httpActionExecutedContext.ActionContext.ActionDescriptor.ActionName;
                StatsDHelper.LogCount(string.Format("{0}.{1}", actionName, (int)httpActionExecutedContext.Response.StatusCode));
            }
        }
    }
}