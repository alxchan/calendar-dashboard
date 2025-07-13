using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace CalendarDashboard.Services
{
    public class GoogleCalendarService
    {
        private readonly CalendarService calendarService;
        public GoogleCalendarService(CalendarService service)
        {
            calendarService = service;
        }

        public async Task<IList<Event>>GetUpcomingEvents() {
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
