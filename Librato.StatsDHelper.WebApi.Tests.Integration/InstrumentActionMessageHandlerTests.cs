using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using FluentAssertions;
using Librato.StatsDHelper.WebApi.MessageHandlers;
using NUnit.Framework;

namespace Librato.StatsDHelper.WebApi.Tests.Integration
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
        public async void when_instrumenting_response_status_code_message_should_be_sent()
        {
            await _handler.Run(HttpActionExecutedContext.Request);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => Regex.IsMatch(o, "^ApplicationName.requests#(.*)http_status=200(.*):1|c$")).Should().BeTrue();
        }

        [Test]
        public async void when_instrumenting_action_name_should_be_sent()
        {
            await _handler.Run(HttpActionExecutedContext.Request);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => Regex.IsMatch(o, "^ApplicationName.requests#(.*)action=actionname(.*):1|c$")).Should().BeTrue();
        }

        [Test]
        public async void when_instrumenting_controller_name_should_be_sent()
        {
            await _handler.Run(HttpActionExecutedContext.Request);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => Regex.IsMatch(o, "^ApplicationName.requests#(.*)controller=controllername(.*):1|c$")).Should().BeTrue();
        }

        [Test]
        public async void when_instrumenting_response_latency_message_should_be_sent()
        {
            await _handler.Run(HttpActionExecutedContext.Request);

            var result = await ListenForTwoStatsDMessages();

            result.Any(o => Regex.IsMatch(o, "^ApplicationName.requests.latency#(.*):(\\d+)|ms$")).Should().BeTrue();
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
