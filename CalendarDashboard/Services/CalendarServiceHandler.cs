using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace CalendarDashboard.Services
{
    public class CalendarServiceHandler
    { 
        public CalendarService GenerateCalendarService(String accessToken) {

            var credential = GoogleCredential.FromAccessToken(accessToken);
            var calendarService = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Integrated Calendar Dashboard"
            });
            return calendarService;
        } 
    }
}
