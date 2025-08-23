
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace CalendarDashboard.Services
{
    public class AccessTokenHandler : DelegatingHandler
    {
        IHttpClientFactory httpClientFactory;
        TokenServiceHandler tokenServiceHandler;

        public record AntiforgeryTokenResponse(string Token);
        public AccessTokenHandler(TokenServiceHandler tokenServiceHandler, IHttpClientFactory httpClientFactory)
        { 
            this.tokenServiceHandler = tokenServiceHandler;
            this.httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var handler = GetInnermostHandler() as HttpClientHandler;
            var authCookieValue = tokenServiceHandler.GetAuthCookie();
            var authCookie = new Cookie(".AspNetCore.Cookies", authCookieValue)
            {
                Domain = request.RequestUri!.Host,
                Path = "/"
            };
            handler!.CookieContainer.Add(authCookie);

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
        private HttpMessageHandler GetInnermostHandler()
        {
            HttpMessageHandler handler = this;
            while (handler is DelegatingHandler delegatingHandler)
            {
                handler = delegatingHandler.InnerHandler!;
            }
            return handler!;
        }
    }
}
