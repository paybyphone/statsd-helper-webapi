using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            context.InstrumentResponse();

            base.OnException(context);
        }
    }
}