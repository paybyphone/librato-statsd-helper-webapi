using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using StatsDHelper;

namespace Librato.StatsDHelper.WebApi.Services
{
    internal class InstrumentationService : IInstrumentationService
    {
        private readonly IAppSettings _appSettings;
        private readonly IStatsDHelper _statsDHelper = global::StatsDHelper.StatsDHelper.Instance;

        private readonly IDictionary<string, Func<HttpActionDescriptor, object>> _templateRegistry = new Dictionary<string, Func<HttpActionDescriptor, object>>
        {
            {"action", actionDescriptor => actionDescriptor.ActionName},
            {"controller", actionDescriptor => actionDescriptor.ControllerDescriptor.ControllerName}
        };

        public InstrumentationService()
        {
            _appSettings = new AppSettings();
        }

        internal InstrumentationService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void TimeRequest(HttpRequestMessage request)
        {
            var requestStopwatch = new Stopwatch();
            request.Properties.Add(Constants.StopwatchKey, requestStopwatch);
            requestStopwatch.Start();
        }

        public void InstrumentResponse(HttpActionExecutedContext httpActionExecutedContext)
        {
            InstrumentResponse(httpActionExecutedContext.Response);
        }

        public void InstrumentResponse(HttpResponseMessage response)
        {
            if (response != null && response.RequestMessage != null)
            {
                var actionDescriptor = response.RequestMessage.GetActionDescriptor();
                if (actionDescriptor != null)
                {
                    var tags = _templateRegistry.Select(t => new KeyValuePair<string, string>(t.Key, t.Value(actionDescriptor).ToString().ToLowerInvariant())).ToList();
                    tags.Add(new KeyValuePair<string, string>("http_status", ((int)response.StatusCode).ToString()));

                    var formattedTags = string.Join(",", tags.Select(t => t.Key + "=" + t.Value));

                    _statsDHelper.LogCount(string.Format("requests#{0}", formattedTags));

                    var requestStopwatch = response.RequestMessage.Properties[Constants.StopwatchKey] as Stopwatch;

                    if (requestStopwatch != null)
                    {
                        requestStopwatch.Stop();
                        _statsDHelper.LogTiming(string.Format("requests.latency#{0}", formattedTags),
                            (long) requestStopwatch.Elapsed.TotalMilliseconds);

                        if (_appSettings.GetBoolean(Constants.Configuration.LatencyHeaderEnabled))
                        {
                            response.Headers.Add("X-ExecutionTime",
                                string.Format("{0}ms", Math.Round(requestStopwatch.Elapsed.TotalMilliseconds)));
                        }
                    }
                }
            }
        }
    }
}