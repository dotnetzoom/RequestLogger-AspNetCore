using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;

namespace RequestLogger.AppCode
{
    public static class WebHelperConfigurationExtensions
    {
        /// <summary>
        /// Add web helper service to IServiceCollection services
        /// </summary>
        /// <param name="services">services</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddWebHelper(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddHttpContextAccessor();

            return services.AddSingleton<IWebHelper, WebHelper>();
        }
    }

    /// <summary>
    /// Represents a web helper
    /// </summary>
    public partial class WebHelper : IWebHelper
    {
        #region Fields 

        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public WebHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check whether current HTTP request is available
        /// </summary>
        /// <returns>True if available; otherwise false</returns>
        protected virtual bool IsRequestAvailable()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return false;

            try
            {
                if (_httpContextAccessor.HttpContext.Request == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get URL referrer if exists
        /// </summary>
        /// <returns>URL referrer</returns>
        public virtual string GetUrlReferrer()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            //URL referrer is null in some case (for example, in IE 8)
            return _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];
        }

        /// <summary>
        /// Get IP address from HTTP context
        /// </summary>
        /// <returns>String of IP address</returns>
        public virtual string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var clientIp = string.Empty;
            try
            {
                var headers = _httpContextAccessor.HttpContext.Request.Headers;

                //first try to get IP address from the headers
                if (headers != null)
                {
                    //if you are using CloudFlare try to CF-Connecting-IP or True-Client-IP (Enterprise plan only)
                    //https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers-
                    clientIp = headers["CF-Connecting-IP"].FirstOrDefault() ?? headers["True-Client-IP"].FirstOrDefault();


                    //the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
                    //connecting to a web server through an HTTP proxy or load balancer
                    //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-For
                    //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Forwarded
                    clientIp ??= headers["X-FORWARDED-FOR"].FirstOrDefault();
                }

                //if this header not exists try get connection remote IP address
                if (string.IsNullOrEmpty(clientIp) && _httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null)
                    clientIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch
            {
                return string.Empty;
            }

            //some of the validation
            if (clientIp != null && clientIp.Equals(IPAddress.IPv6Loopback.ToString(), StringComparison.InvariantCultureIgnoreCase))
                clientIp = IPAddress.Loopback.ToString();

            //"TryParse" doesn't support IPv4 with port number
            if (IPAddress.TryParse(clientIp ?? string.Empty, out var ip))
                //IP address is valid 
                clientIp = ip.ToString();
            else if (!string.IsNullOrEmpty(clientIp))
                //remove port
                clientIp = clientIp.Split(':').FirstOrDefault();

            return clientIp;
        }

        /// <summary>
        /// Get the raw path and full query of request
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Raw URL</returns>
        public virtual string GetRawUrl()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var request = _httpContextAccessor.HttpContext.Request;

            //first try to get the raw target from request feature
            //note: value has not been UrlDecoded
            var rawUrl = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

            //or compose raw URL manually
            if (string.IsNullOrEmpty(rawUrl))
                rawUrl = $"{request.PathBase}{request.Path}{request.QueryString}";

            return rawUrl;
        }

        /// <summary>
        /// Gets whether the request is made with AJAX 
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>Result</returns>
        public virtual bool IsAjaxRequest()
        {
            if (!IsRequestAvailable())
                return false;

            if (_httpContextAccessor.HttpContext.Request.Headers == null)
                return false;

            return _httpContextAccessor.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        #endregion
    }
}
