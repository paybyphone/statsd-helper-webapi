using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using NUnit.Framework;
using StatsDHelper.WebApi.Filters;

namespace StatsDHelper.WebApi.Tests.Integration
{
    [TestFixture]
    public class HttpActionExecutedContextExtensionsTests
    {
        private HttpActionExecutedContext _httpActionExecutedContext;
        private static CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private UdpClient _udpClient;
        private IPEndPoint _ipEndPoint;

        [TestFixtureSetUp]
        public void StartUdpListener()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _httpActionExecutedContext = SetUpFakeHttpActionContext();
        }

        private HttpActionExecutedContext SetUpFakeHttpActionContext()
        {
            var port = int.Parse(ConfigurationManager.AppSettings["StatsD.Port"]);

            var actionContext = new HttpActionContext { ActionDescriptor = new FakeActionDescriptor() };

            var httpActionExecutedContext = new HttpActionExecutedContext
            {
                ActionContext = actionContext,
                Response = new HttpResponseMessage()
            };

            _udpClient = new UdpClient(port);
            _ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            return httpActionExecutedContext;
        }

        private async Task<string> ListenForStatsDMessage()
        {
            return await Task.Run(() =>
            {
                var udpData = _udpClient.Receive(ref _ipEndPoint);
                var stringData = System.Text.Encoding.UTF8.GetString(udpData);

                return stringData;
            }, _cancellationToken);
        }

        [TestFixtureTearDown]
        public void CancelUdpListener()
        {
            _udpClient.Close();
            _cancellationTokenSource.Cancel();
        }

        [Test]
        public async void when_instrumenting_response_statsd_message_should_be_sent()
        {
            _httpActionExecutedContext.InstrumentResponse();

            var result = await ListenForStatsDMessage();

            result.Should().Contain("ApplicationName.ActionName.200:1|c");
        }

        [Test]
        public async void when_include_controller_name_is_enabled_then_the_metric_should_include_the_controller_name()
        {
            _httpActionExecutedContext.InstrumentResponse(template: "{controller}.{action}");

            var result = await ListenForStatsDMessage();

            result.Should().Contain("ApplicationName.ControllerName.ActionName.200:1|c");
        }
    }
}