using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using FluentAssertions;
using NUnit.Framework;
using FakeItEasy;
using StatsDHelper.WebApi.MessageHandlers;

namespace StatsDHelper.WebApi.Tests.Integration
{
    [TestFixture]
    internal class InstrumentActionMessageHandlerTests : BaseInstrumentationTests
    {
        private HandlerAccessor _handler;

        [SetUp]
        public void Setup()
        {
            _handler = new HandlerAccessor(HttpActionExecutedContext)
            {
                InnerHandler = new InstrumentActionMessageHandler
                {
                    InnerHandler = new FakeHandler(HttpActionExecutedContext)
                }
            };
        }

        [Test]
        public async Task when_instrumenting_response_status_code_message_should_be_sent()
        {
            await _handler.Run(HttpActionExecutedContext.Request);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => o.Contains("ApplicationName.actionname.200:1|c")).Should().BeTrue();
        }

        [Test]
        public async Task when_instrumenting_response_latency_message_should_be_sent()
        {
            await _handler.Run(HttpActionExecutedContext.Request);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => o.Contains("ApplicationName.actionname.latency:") && o.EndsWith("|ms")).Should().BeTrue();
        }

        [Test]
        public async Task when_template_includes_controller_name_then_the_metric_should_include_the_controller_name()
        {
            _handler.InnerHandler = new InstrumentActionMessageHandler("{controller}.{action}")
            {
                InnerHandler = new FakeHandler(HttpActionExecutedContext)
            };

            await _handler.Run(HttpActionExecutedContext.Request);

            var result = await ListenForTwoStatsDMessages();

            result.First().Should().Contain("ApplicationName.controllername.actionname.200:1|c");
        }

        private class HandlerAccessor : DelegatingHandler
        {
            private readonly CancellationTokenSource _tokenSource;

            public HandlerAccessor(HttpActionExecutedContext context)
            {
                _tokenSource = new CancellationTokenSource();
            }

            public Task Run(HttpRequestMessage request)
            {
                return SendAsync(request, _tokenSource.Token);
            }
        }

        private class FakeHandler : HttpMessageHandler
        {
            private readonly HttpActionExecutedContext _context;

            public FakeHandler(HttpActionExecutedContext context)
            {
                _context = context;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_context.Response);
            }

        }
    }
}
