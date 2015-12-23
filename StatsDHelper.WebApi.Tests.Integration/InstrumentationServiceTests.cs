using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace StatsDHelper.WebApi.Tests.Integration
{
    [TestFixture]
    class InstrumentationServiceTests : BaseInstrumentationTests
    {
        [SetUp]
        public void Setup()
        {
            AddRequestStopwatch(HttpActionExecutedContext);
        }

        [Test]
        public async void when_instrumenting_response_status_code_message_should_be_sent()
        {
            LatencyHeaderOff();

            InstrumentationService.InstrumentResponse(HttpActionExecutedContext);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => o.Contains("ApplicationName.actionname.200:1|c")).Should().BeTrue();
        }

        private void LatencyHeaderOff()
        {
            A.CallTo(() => AppSettings.GetBoolean(Constants.Configuration.LatencyHeaderEnabled)).Returns(false);
        }

        private void LatencyHeaderOn()
        {
            A.CallTo(() => AppSettings.GetBoolean(Constants.Configuration.LatencyHeaderEnabled)).Returns(true);
        }

        [Test]
        public async void when_instrumenting_response_latency_message_should_be_sent()
        {
            LatencyHeaderOff();

            InstrumentationService.InstrumentResponse(HttpActionExecutedContext);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => o.Contains("ApplicationName.actionname.latency:") && o.EndsWith("|ms")).Should().BeTrue();
        }

        [Test]
        public async void when_action_name_is_mixed_case_then_it_will_be_changed_to_lowercase_for_the_metric_name()
        {
            LatencyHeaderOff();

            HttpActionExecutedContext.ActionContext.ActionDescriptor.As<FakeActionDescriptor>().SetActionName("AcTiOnNaMe");

            InstrumentationService.InstrumentResponse(HttpActionExecutedContext);

            var result = await ListenForTwoStatsDMessages();

            result.First().Should().Contain("ApplicationName.actionname.200:1|c");
        }


        [Test]
        public async void when_include_controller_name_is_enabled_then_the_metric_should_include_the_controller_name()
        {
            LatencyHeaderOff();

            InstrumentationService.InstrumentResponse(HttpActionExecutedContext, template: "{controller}.{action}");

            var result = await ListenForTwoStatsDMessages();

            result.First().Should().Contain("ApplicationName.controllername.actionname.200:1|c");
        }

        [Test]
        public void when_latency_headers_on_then_execution_time_is_added_to_the_reponse_headers()
        {
            LatencyHeaderOn();

            InstrumentationService.InstrumentResponse(HttpActionExecutedContext, template: "{controller}.{action}");

            HttpActionExecutedContext.Response.Headers.Any(o => o.Key.Contains("X-ExecutionTime")).Should().BeTrue();
        }

        [Test]
        public void when_latency_headers_off_then_execution_time_is_not_added_to_the_reponse_headers()
        {
            LatencyHeaderOff();

            InstrumentationService.InstrumentResponse(HttpActionExecutedContext, template: "{controller}.{action}");

            HttpActionExecutedContext.Response.Headers.Any(o => o.Key.Contains("X-ExecutionTime")).Should().BeFalse();
        }
    }
}