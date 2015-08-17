using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly string _template;

        public InstrumentStatusCodeExceptionFilterAttribute(string template = "{action}")
        {
            _template = template;
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            context.InstrumentResponse(_template);

            base.OnException(context);
        }
    }
}