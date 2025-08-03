using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;

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


        public async Task<IList<string>> GetCalendarIds() {
            var authResult = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            var accessToken = authResult.Properties!.GetTokenValue("access_token");
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken!);
            var request = calendarService.CalendarList.List();
            var calendarEntries = await request.ExecuteAsync();
            var calendarIds = new List<string>(); 
            foreach (var item in calendarEntries.Items)
            {
                calendarIds.Add(item.Id);
            };

            return calendarIds;
        }

        public async Task<IList<Event>?> GetUpcomingEvents(string calendarId) {

            var authResult = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            var accessToken = authResult.Properties!.GetTokenValue("access_token");
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken!);
            var request = calendarService.Events.List(calendarId);
            request.TimeMinDateTimeOffset = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            
            var events = await request.ExecuteAsync();

            return events.Items;

        }

        public async Task<Event?> AddEvent(string? summary, string? description, EventDateTime? start, EventDateTime? end, List<EventAttendee>? attendees) {
            //Later add parameters to fill out event fields
            var authResult = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            var accessToken = authResult.Properties!.GetTokenValue("access_token");
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken!);
            Event newEvent = new Event
            {
                Summary = summary,
                Description = description,
                Start = start,
                End = end,
                Attendees = attendees

            };
            var request = calendarService.Events.Insert(newEvent, "primary");
            var createdRequest = await request.ExecuteAsync();
            return newEvent;  
          
        }

        public async Task<Event?> UpdateEvent(string eventId, string? summary, string? description, EventDateTime? start, EventDateTime? end, List<EventAttendee>? attendees) {
            var authResult = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            var accessToken = authResult.Properties!.GetTokenValue("access_token");
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken!);
            var prevEvent = calendarService.Events.Get("primary", eventId).Execute();
            Console.WriteLine(summary);
            Event updateEvent = new Event
            {
                Summary = !string.IsNullOrEmpty(summary) ? summary : prevEvent.Summary,
                Description = !string.IsNullOrEmpty(description) ? description : prevEvent.Description,
                Start = start ?? prevEvent.Start,
                End = end ?? prevEvent.End,
                Attendees = attendees ?? prevEvent.Attendees

            };

            var request = calendarService.Events.Update(updateEvent, "primary", eventId);
            var updateRequest = await request.ExecuteAsync();
            return updateEvent;
        }

        public async Task<Boolean> DeleteEvent(string eventId) {
            var authResult = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            var accessToken = authResult.Properties!.GetTokenValue("access_token");
            var calendarService = calendarServiceHandler.GenerateCalendarService(accessToken!);

            var request = calendarService.Events.Delete("primary", eventId);
            try
            {
                var deleteRequest = await request.ExecuteAsync();
            }
            catch { return false; }
            return true;
        }

    }
}
