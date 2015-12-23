using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FakeItEasy;
using NUnit.Framework;
using StatsDHelper.WebApi.Services;

namespace StatsDHelper.WebApi.Tests.Integration
{
    abstract class BaseInstrumentationTests
    {
        protected HttpActionExecutedContext HttpActionExecutedContext;
        protected static CancellationTokenSource CancellationTokenSource;
        protected CancellationToken CancellationToken;

        private UdpClient _udpClient;
        protected IInstrumentationService InstrumentationService;
        protected IAppSettings AppSettings;

        [SetUp]
        public void SetUp()
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            HttpActionExecutedContext = GetFakeHttpActionContext();

            AppSettings = A.Fake<IAppSettings>();

            InstrumentationService = new InstrumentationService(AppSettings);

            var port = int.Parse(ConfigurationManager.AppSettings["StatsD.Port"]);
            _udpClient = new UdpClient(port);
        }

        protected HttpActionExecutedContext GetFakeHttpActionContext()
        {
            var actionContext = new HttpActionContext { ActionDescriptor = new FakeActionDescriptor(), ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage() } };
            actionContext.Request.Properties.Add("MS_HttpActionDescriptor", actionContext.ActionDescriptor);
            var httpActionExecutedContext = new HttpActionExecutedContext
            {
                ActionContext = actionContext,
                Response = new HttpResponseMessage
                {
                    RequestMessage = actionContext.Request
                }
            };
            return httpActionExecutedContext;
        }

        protected void AddRequestStopwatch(HttpActionExecutedContext httpActionExecutedContext)
        {
            var requestStopwatch = new Stopwatch();
            httpActionExecutedContext.Request.Properties.Add(Constants.StopwatchKey, requestStopwatch);
            requestStopwatch.Start();
        }

        protected async Task<List<string>> ListenForTwoStatsDMessages()
        {
            return await Task.Run(async () =>
            {
                var result = new List<UdpReceiveResult>();

                while (result.Count != 2)
                {
                    result.Add(await _udpClient.ReceiveAsync());
                }

                return result.Select(o => Encoding.UTF8.GetString(o.Buffer)).ToList();


            }, CancellationToken);
        }

        [TearDown]
        public void CancelUdpListener()
        {
            _udpClient.Close();
            CancellationTokenSource.Cancel();
        }        
    }
}