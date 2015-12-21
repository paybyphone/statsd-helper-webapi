using System.Net.Http;
using System.Web.Http.Filters;

namespace StatsDHelper.WebApi.Services
{
    internal interface IInstrumentationService
    {
        void TimeRequest(HttpRequestMessage request);
        void InstrumentResponse(HttpActionExecutedContext httpActionExecutedContext, string template = "{action}");
        void InstrumentResponse(HttpResponseMessage response, string template = "{action}");
    }
}