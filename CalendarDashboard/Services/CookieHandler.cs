using System.Net;
using Google.Apis.Http;
using Microsoft.AspNetCore.Antiforgery;
using static CalendarDashboard.Services.AccessTokenHandler;

namespace CalendarDashboard.Services
{
    public class CookieHandler
    {
        private IHttpContextAccessor httpContextAccessor;
        private readonly IAntiforgery antiforgery;

        public CookieHandler(IHttpContextAccessor httpContextAccessor, IAntiforgery antiforgery)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.antiforgery = antiforgery;
        }

        //protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{
        //    var httpContext = httpContextAccessor.HttpContext;


        //    if (httpContext != null && httpContext.Request.Headers.TryGetValue("Cookie", out var cookie))
        //    {

        //        request.Headers.Add("Cookie", cookie.ToString());
        //    }

        //    return await base.SendAsync(request, cancellationToken);
        //}

        public static HttpClientHandler AttachCookie(IServiceProvider sp, string uri)
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            var context = httpContextAccessor.HttpContext;
            if (context != null && context.Request.Cookies.TryGetValue(".AspNetCore.Cookies", out var authCookie))
            {
                handler.CookieContainer.Add(
                    new Uri(uri),
                    new Cookie(".AspNetCore.Cookies", authCookie) { Secure = true, HttpOnly = true }
                );
            }

            //REMOVE OUTSIDE TESTING
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return handler;

        }
    }
}


