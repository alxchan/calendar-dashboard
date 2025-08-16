
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;

namespace CalendarDashboard.Services
{
    public class AccessTokenHandler : DelegatingHandler
    {
        IHttpContextAccessor httpContextAccessor;
        IHttpClientFactory httpClientFactory;
        TokenServiceHandler tokenServiceHandler;
        IAntiforgery antiforgery;


        public record AntiforgeryTokenResponse(string Token);
        public AccessTokenHandler(IHttpContextAccessor httpContextAccessor, TokenServiceHandler tokenServiceHandler, IAntiforgery antiforgery, IHttpClientFactory httpClientFactory)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.tokenServiceHandler = tokenServiceHandler;
            this.antiforgery = antiforgery;
            this.httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var authResult = await httpContext!.AuthenticateAsync();
            var accessToken = authResult.Principal!.FindFirstValue("access_token");

            var response = await base.SendAsync(request, cancellationToken);


            if (response.StatusCode != HttpStatusCode.OK && httpContext != null)
            {
                var refreshToken = httpContext.User.FindFirst("refresh_token")?.Value;

                if (!string.IsNullOrEmpty(refreshToken))
                {

                    var newAccessToken = await tokenServiceHandler.RefreshAccessToken(refreshToken);

                    if (!string.IsNullOrEmpty(newAccessToken))
                    {

                        var claimsIdentity = (ClaimsIdentity)httpContext.User.Identity!;
                        var oldAccessTokenClaim = claimsIdentity.FindFirst("access_token");
                        if (oldAccessTokenClaim != null)
                        {
                            claimsIdentity.RemoveClaim(oldAccessTokenClaim);
                        }
                        claimsIdentity.AddClaim(new Claim("access_token", newAccessToken));

                        var originalProperties = await httpContext.AuthenticateAsync();
                        await httpContext.SignInAsync(httpContext.User, originalProperties.Properties);
                        var claims = httpContext.User.Claims;
                        var newToken = claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                        var service = claims.FirstOrDefault(c => c.Type == "service")?.Value;
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            request.Headers.Remove("Cookie");
                            request.Headers.Add("Cookie", $".AspNetCore.Cookies=access_token={newToken};email={email};service={service}");

                        }

                        var token = antiforgery.GetAndStoreTokens(httpContext!);
                        return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                        {
                            RequestMessage = request
                        };
                    }
                }
            }

            return response;
        }
    }
}
