using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Services
{
    public interface IInstrumentationService
    {
        void InstrumentResponse(HttpActionExecutedContext httpActionExecutedContext, string template = "{action}");
    }
}