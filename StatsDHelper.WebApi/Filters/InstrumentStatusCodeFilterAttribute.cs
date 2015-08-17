using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeFilterAttribute : ActionFilterAttribute
    {
        private readonly string _template;

        public InstrumentStatusCodeFilterAttribute(string template = "{action}")
        {
            _template = template;
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            actionExecutedContext.InstrumentResponse(_template);

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}