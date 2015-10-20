using System.Web.Http.Filters;
using StatsDHelper.WebApi.Services;

namespace StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly string _template;
        private readonly IInstrumentationService _instrumentationService;

        public InstrumentStatusCodeExceptionFilterAttribute(string template = "{action}")
        {
            _template = template;
            _instrumentationService = new InstrumentationService();
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            _instrumentationService.InstrumentResponse(context, _template);

            base.OnException(context);
        }
    }
}