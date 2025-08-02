using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CalendarDashboard.Services
{
    public class GoogleCalendarService
    {
        private readonly CalendarServiceHandler calendarServiceHandler;
        private readonly TokenServiceHandler tokenServiceHandler;
        private readonly IHttpContextAccessor httpContextAccessor;
        public GoogleCalendarService(CalendarServiceHandler calendarServiceHandler, TokenServiceHandler tokenServiceHandler, IHttpContextAccessor httpContext)
        {
            this.calendarServiceHandler = calendarServiceHandler;
            this.tokenServiceHandler = tokenServiceHandler;
            this.httpContextAccessor = httpContext;
        }

        public async Task<IList<Event>?> GetUpcomingEvents() {

            var authResult = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            var accessToken = authResult.Properties!.GetTokenValue("access_token");
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken!);
            var request = calendarService.Events.List("primary");
            request.TimeMinDateTimeOffset = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            
            var events = await request.ExecuteAsync();

            return events.Items;

        }

        public async Task<Event?> AddEvent() {
            //Later add parameters to fill out event fields
            var authResult = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            var accessToken = authResult.Properties!.GetTokenValue("access_token");
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken!);
            Event newEvent = new Event
            {
                Summary = "Test Event",
                Description = "This is an event added by an API endpoint",
                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = DateTime.Now.AddHours(1),
                    TimeZone = "America/Toronto"
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = DateTime.Now.AddHours(5),
                    TimeZone = "America/Toronto"
                },
                Attendees = [new EventAttendee() { Email = "ADD EMAIL HERE", Organizer = true }, new EventAttendee { Email = "ADD EMAIL HERE"}]

            };
            var request = calendarService.Events.Insert(newEvent, "primary");
            var createdRequest = await request.ExecuteAsync();
            return newEvent;  
          
        }

    }
}
