using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace CalendarDashboard.Services
{
    public class GoogleCalendarService
    {
        private readonly CalendarServiceHandler calendarServiceHandler;
        private readonly TokenServiceHandler tokenServiceHandler;
        public GoogleCalendarService(CalendarServiceHandler calendarServiceHandler, TokenServiceHandler tokenServiceHandler)
        {
            this.calendarServiceHandler = calendarServiceHandler;
            this.tokenServiceHandler = tokenServiceHandler;
        }

        public async Task<IList<Event>?> GetUpcomingEvents() {

            var accessToken = tokenServiceHandler.GetDecryptedAccessToken();
            if (accessToken == null) {
                Console.WriteLine("Cannot find in DB!");
                return null; }
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken);
            var request = calendarService.Events.List("primary");
            request.TimeMinDateTimeOffset = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            
            var events = await request.ExecuteAsync();

            return events.Items;

        }

    }
}
