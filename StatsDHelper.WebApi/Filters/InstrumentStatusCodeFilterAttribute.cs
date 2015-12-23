using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using StatsDHelper.WebApi.Services;

namespace StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeFilterAttribute : ActionFilterAttribute
    {
        private readonly string _template;
        private readonly IInstrumentationService _instrumentationService;

        public InstrumentStatusCodeFilterAttribute(string template = "{action}")
        {
            _template = template;
            _instrumentationService = new InstrumentationService();
        }

        internal InstrumentStatusCodeFilterAttribute(string thing, string template = "{action}")
        {
            
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            _instrumentationService.TimeRequest(actionContext.Request);

            base.OnActionExecuting(actionContext);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _instrumentationService.InstrumentResponse(actionExecutedContext, _template);

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}