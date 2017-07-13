using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Librato.StatsDHelper.WebApi.Services;

namespace Librato.StatsDHelper.WebApi.Filters
{
    public class InstrumentStatusCodeFilterAttribute : ActionFilterAttribute
    {
        private readonly IInstrumentationService _instrumentationService;

        public InstrumentStatusCodeFilterAttribute()
        {
            _instrumentationService = new InstrumentationService();
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            _instrumentationService.TimeRequest(actionContext.Request);

            base.OnActionExecuting(actionContext);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _instrumentationService.InstrumentResponse(actionExecutedContext);

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}