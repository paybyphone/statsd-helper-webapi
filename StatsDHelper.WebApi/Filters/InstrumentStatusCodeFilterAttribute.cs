using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeFilterAttribute : ActionFilterAttribute
    {
        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            actionExecutedContext.InstrumentResponse();

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}