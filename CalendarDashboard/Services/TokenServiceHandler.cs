using System.Security.Claims;
using CalendarDashboard.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace CalendarDashboard.Services
{
    public class TokenServiceHandler
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly CalendarDBContext dbContext;

        public TokenServiceHandler(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, CalendarDBContext dbContext) 
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
        }

        public string? GetDecryptedAccessToken()
        {
            var claims = httpContextAccessor.HttpContext?.User.Claims;
            var googleUserId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Google User ID is: " + googleUserId);
            //Retrieves and decrypts the token from currently signed in user
            var data = dbContext.UserTokens.FirstOrDefault(x => x.UserId == googleUserId);
            if (data == null) { return null; }
            else { return AesGcmEncryptor.decrypt(data.AccessToken, Convert.FromBase64String(configuration["API_KEY"]!)); }
        }
    }
}
