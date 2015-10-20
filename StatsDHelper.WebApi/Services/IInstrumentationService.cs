using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Services
{
    internal interface IInstrumentationService
    {
        void InstrumentResponse(HttpActionExecutedContext httpActionExecutedContext, string template = "{action}");
    }
}