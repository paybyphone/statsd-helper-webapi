using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
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
        public class FakeActionDescriptor : HttpActionDescriptor
        {
            public FakeActionDescriptor() : base(new HttpControllerDescriptor(new HttpConfiguration(), "ControllerName", typeof(object)))
            {
            }

            public override Collection<HttpParameterDescriptor> GetParameters()
            {
                throw new NotImplementedException();
            }

            public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override string ActionName
            {
                get { return "ActionName"; }
            }

            public override Type ReturnType
            {
                get { throw new NotImplementedException(); }
            }
        }

        [Test]
        public void when_instrumenting_response_statsd_message_should_be_sent()
        {
            Task<byte[]> task;
            var httpActionExecutedContext = SetUpFakeHttpActionContext(out task);

            httpActionExecutedContext.InstrumentResponse();

            task.Wait();

            var result = System.Text.Encoding.UTF8.GetString(task.Result);

            result.Should().Contain("ApplicationName.ActionName.200:1|c");
        }

        private static HttpActionExecutedContext SetUpFakeHttpActionContext(out Task<byte[]> task)
        {
            var port = int.Parse(ConfigurationManager.AppSettings["StatsD.Port"]);

            var actionContext = new HttpActionContext {ActionDescriptor = new FakeActionDescriptor()};

            var httpActionExecutedContext = new HttpActionExecutedContext
            {
                ActionContext = actionContext,
                Response = new HttpResponseMessage()
            };

            task = Task.Factory.StartNew(() => ListenForUdpOnStatsDPort(port));
            return httpActionExecutedContext;
        }

        [Test]
        public void when_include_controller_name_is_enabled_then_the_metric_should_include_the_controller_name()
        {
            Task<byte[]> task;
            var httpActionExecutedContext = SetUpFakeHttpActionContext(out task);

            httpActionExecutedContext.InstrumentResponse(includeControllerName: true);

            task.Wait();

            var result = System.Text.Encoding.UTF8.GetString(task.Result);

            result.Should().Contain("ApplicationName.ControllerName.ActionName.200:1|c");
        }

        private static byte[] ListenForUdpOnStatsDPort(int port)
        {
            var udpClient = new UdpClient(port);
            var ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);
            var data = udpClient.Receive(ref ipEndPoint);

            return data;
        }
    }
}