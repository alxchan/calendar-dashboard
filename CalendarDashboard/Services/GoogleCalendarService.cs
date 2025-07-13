using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;

namespace CalendarDashboard.Services
{
    public class GoogleCalendarService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GoogleCalendarService(IConfiguration configuration)
        {
            _clientId = configuration["Google:ClientId"];
            _clientSecret = configuration["Google:ClientSecret"];
        }

        public async Task<CalendarService> GetCalendarAPIServiceAsync(string userId) 
        {
            var scopes = new[] { CalendarService.Scope.Calendar };

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret}, 
                scopes, 
                userId, 
                CancellationToken.None
            );

            return new CalendarService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Integrated Calendar Dashboard"
            });

        }


    }
}
