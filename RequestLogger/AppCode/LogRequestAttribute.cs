using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;

namespace RequestLogger.AppCode
{
    public sealed class LogRequestAttribute : TypeFilterAttribute
    {
        public LogRequestAttribute()
            : base(typeof(LogRequestFilterAttribute))
        {
        }

        private sealed class LogRequestFilterAttribute : ActionFilterAttribute
        {
            private readonly IDiagnosticContext _diagnosticContext;
            private readonly IWebHelper _webHelper;

            public LogRequestFilterAttribute(IDiagnosticContext diagnosticContext, IWebHelper webHelper)
            {
                _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
                _webHelper = webHelper ?? throw new ArgumentNullException(nameof(webHelper));
            }

            public override void OnActionExecuting(ActionExecutingContext context)
            {
                //Mark this log as a action LogRequest to filtering
                _diagnosticContext.Set(nameof(LogRequestAttribute), null);

                _diagnosticContext.Set("ModelState.IsValid", context.ModelState.IsValid);
                var errors = context.ModelState.ToDictionary(p => p.Key, p => p.Value.Errors.Select(q => q.ErrorMessage));
                _diagnosticContext.Set("ModelState.Errors", errors);
                _diagnosticContext.Set("ActionArguments", context.ActionArguments, true);

                _diagnosticContext.Set("Request.Host", context.HttpContext.Request.Host, false);
                _diagnosticContext.Set("Request.Query", context.HttpContext.Request.Query, true);
                //_diagnosticContext.Set("Request.QueryString", context.HttpContext.Request.QueryString, true);
                _diagnosticContext.Set("Request.Headers", context.HttpContext.Request.Headers, true);
                _diagnosticContext.Set("Request.Cookies", context.HttpContext.Request.Cookies, true);
                _diagnosticContext.Set("Request.RouteValues", context.HttpContext.Request.RouteValues, true);

                _diagnosticContext.Set("User.Identity.Name", context.HttpContext.User.Identity.Name, true);
                _diagnosticContext.Set("User.Identity.IsAuthenticated", context.HttpContext.User.Identity.IsAuthenticated, true);
                _diagnosticContext.Set("User.Identity.Claims", context.HttpContext.User.Claims, true);
                //_diagnosticContext.Set("User.Identity.AuthenticationType", context.HttpContext.User.Identity.AuthenticationType, true);

                _diagnosticContext.Set("Response.Headers", context.HttpContext.Response.Headers, true);
                _diagnosticContext.Set("Response.Cookies", context.HttpContext.Response.Cookies, true);

                _diagnosticContext.Set("IpAddress", _webHelper.GetCurrentIpAddress());
                _diagnosticContext.Set("UrlReferrer", _webHelper.GetUrlReferrer());
                //_diagnosticContext.Set("RawUrl", _webHelper.GetRawUrl());

                base.OnActionExecuting(context);
            }
        }
    }
}
