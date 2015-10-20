using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using StatsDHelper.WebApi.Filters;

namespace StatsDHelper.WebApi.Tests.Integration
{
    [TestFixture]
    public class HttpActionExecutedContextExtensionsTests : BaseHttpActionExecutedContextExtensionsTests
    {
        [Test]
        public async void when_instrumenting_response_status_code_message_should_be_sent()
        {
            HttpActionExecutedContext.InstrumentResponse();

            var result = await ListenForTwoStatsDMessage();

            result.Any(o => o.Contains("ApplicationName.actionname.200:1|c")).Should().BeTrue();
        }

        [Test]
        public async void when_instrumenting_response_latency_message_should_be_sent()
        {
            HttpActionExecutedContext.InstrumentResponse();

            var result = await ListenForTwoStatsDMessage();

            result.Any(o => o.Contains("ApplicationName.actionname.latency:") && o.EndsWith("|ms")).Should().BeTrue();
        }

        [Test]
        public async void when_action_name_is_mixed_case_then_it_will_be_changed_to_lowercase_for_the_metric_name()
        {
            HttpActionExecutedContext.ActionContext.ActionDescriptor.As<FakeActionDescriptor>().SetActionName("AcTiOnNaMe");

            HttpActionExecutedContext.InstrumentResponse();

            var result = await ListenForTwoStatsDMessage();

            result.First().Should().Contain("ApplicationName.actionname.200:1|c");
        }


        [Test]
        public async void when_include_controller_name_is_enabled_then_the_metric_should_include_the_controller_name()
        {
            HttpActionExecutedContext.InstrumentResponse(template: "{controller}.{action}");

            var result = await ListenForTwoStatsDMessage();

            result.First().Should().Contain("ApplicationName.controllername.actionname.200:1|c");
        }

        [Test]
        public void when_response_returns_execution_time_is_added_to_the_headers()
        {
            HttpActionExecutedContext.InstrumentResponse(template: "{controller}.{action}");

            HttpActionExecutedContext.Response.Headers.Any(o => o.Key.Contains("X-ExecutionTime")).Should().BeTrue();
        }
    }
}