using System.Web.Http.Filters;
using Librato.StatsDHelper.WebApi.Services;

namespace Librato.StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IInstrumentationService _instrumentationService;

        public InstrumentStatusCodeExceptionFilterAttribute()
        {
            _instrumentationService = new InstrumentationService();
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            _instrumentationService.InstrumentResponse(context);

            base.OnException(context);
        }
    }
}