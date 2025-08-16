using System.Security.Claims;
using CalendarDashboard.Models;
using Newtonsoft.Json.Linq;

namespace CalendarDashboard.Services
{
    public class TokenServiceHandler
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly CalendarDBContext dbContext;
        private readonly HttpClient client;

        public TokenServiceHandler(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, CalendarDBContext dbContext, HttpClient client) 
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
            this.client = client;
        }

        //public string? GetDecryptedAccessToken()
        //{
        //    var claims = httpContextAccessor.HttpContext?.User.Claims;
        //    var googleUserId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    Console.WriteLine("Google User ID is: " + googleUserId);
        //    //Retrieves and decrypts the token from currently signed in user
        //    var data = dbContext.UserTokens.FirstOrDefault(x => x.UserId == googleUserId);
        //    if (data == null) { return null; }
        //    else { return AesGcmEncryptor.decrypt(data.AccessToken!, Convert.FromBase64String(configuration["API_KEY"]!)); }
        //}

        public string? GetDecryptedRefreshToken()
        {
            var claims = httpContextAccessor.HttpContext?.User.Claims;
            var googleUserId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var data = dbContext.UserTokens.FirstOrDefault(x => x.Email == googleUserId);
            if (data == null) { return null; }
            else
            {
                return AesGcmEncryptor.decrypt(data.RefreshToken!, Convert.FromBase64String(configuration["API_KEY"]!));

            }
        }

        public async Task<string> RefreshAccessToken(string refreshToken)
        {
            //When making requests to refresh access tokens i.e. offline tasks, store the user id in the associated tasks
            var postData = new Dictionary<string, string> {
                {"client_id",  configuration["Google:ClientId"]!},
                {"client_secret", configuration["Google:ClientSecret"]! },
                {"refresh_token", refreshToken! },
                {"grant_type", "refresh_token" }
            };

            var content = new FormUrlEncodedContent(postData);

            HttpResponseMessage response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var tokenData = JObject.Parse(responseContent);
            string newAccessToken = tokenData.Value<string>("access_token")!;

            return newAccessToken;
        }

    }
}
