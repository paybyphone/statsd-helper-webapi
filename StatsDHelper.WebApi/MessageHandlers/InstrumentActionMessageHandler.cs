using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StatsDHelper.WebApi.Services;

namespace StatsDHelper.WebApi.MessageHandlers
{
    public class InstrumentActionMessageHandler : DelegatingHandler
    {
        private readonly string _template;
        private readonly IInstrumentationService _instrumentationService;

        public InstrumentActionMessageHandler(string template = "{action}")
        {
            _template = template;
            _instrumentationService = new InstrumentationService();
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _instrumentationService.TimeRequest(request);

            var response = await base.SendAsync(request, cancellationToken);

            _instrumentationService.InstrumentResponse(response, _template);

            return response;
        }

    }
}
