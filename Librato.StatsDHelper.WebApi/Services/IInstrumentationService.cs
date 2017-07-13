using System.Net.Http;
using System.Web.Http.Filters;

namespace Librato.StatsDHelper.WebApi.Services
{
    internal interface IInstrumentationService
    {
        void TimeRequest(HttpRequestMessage request);
        void InstrumentResponse(HttpActionExecutedContext httpActionExecutedContext);
        void InstrumentResponse(HttpResponseMessage response);
    }
}