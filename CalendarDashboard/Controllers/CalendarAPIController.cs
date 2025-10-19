using System.Net.Http;
using System.Security.Claims;
using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace CalendarDashboard.Controllers
{
    [ApiController]
    [Route("api/calendar")]
    public class CalendarAPIController : Controller
    {
        private readonly CalendarDBContext db;
        private readonly CalendarServiceHandler calendarServiceHandler;
        private readonly TokenServiceHandler tokenServiceHandler;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAntiforgery antiforgery;

        public CalendarAPIController(CalendarDBContext db, CalendarServiceHandler calendarServiceHandler, TokenServiceHandler tokenServiceHandler, IHttpContextAccessor httpContext, IAntiforgery antiforgery)
        {
            this.db = db;
            this.calendarServiceHandler = calendarServiceHandler;
            this.tokenServiceHandler = tokenServiceHandler;
            this.httpContextAccessor = httpContext;
            this.antiforgery = antiforgery;
        }

        [HttpPost("token")]
        public IActionResult GetToken()
        {
            var tokens = antiforgery.GetAndStoreTokens(httpContextAccessor.HttpContext!);
            return Ok(tokens.RequestToken);
        }

        [ValidateAntiForgeryToken]
        [HttpGet("test")]
        public async Task<IActionResult> RetrieveUpcomingEvents()
        {
            try { 
                var calendarService = new GoogleCalendarService(calendarServiceHandler, tokenServiceHandler, httpContextAccessor);
                var email = HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value;
                var calendarIds = await calendarService.GetCalendarIds();
                var calendarId = calendarIds[calendarIds.Count-1];
                var listEvents = await calendarService.GetUpcomingEvents(calendarId);
                var listLocalEvents = new List<LocalEvent>();
                foreach (var item in listEvents!)
                {
                    var existing = db.Events.FirstOrDefault(x => x.Email == email);
                    if (existing != null)
                    {
                        existing.Attendees = (List<EventAttendee>?)(item.Attendees ?? existing.Attendees);
                        existing.Status = item.Status ?? existing.Status;
                        existing.StartTime = item.Start ?? existing.StartTime;
                        existing.EndTime = item.End ?? existing.EndTime;
                        existing.Name = item.Summary ?? existing.Name;
                        existing.Description = item.Description ?? existing.Description;
                        existing.Location = item.Location ?? existing.Location;
                        db.Events.Update(existing);
                        listLocalEvents.Add(new LocalEvent() { Email = email!, EventId = item.Id, CalendarId = calendarId, Name = item.Summary, Description = item.Description, StartTime = item.Start!, EndTime = item.End!, Location = item.Location });
                    }
                    else { 
                        db.Events.Add(new LocalEvent() { Email = email!, EventId = item.Id, CalendarId = calendarId, Name = item.Summary, Description = item.Description, StartTime = item.Start!, EndTime = item.End!, Location = item.Location });
                    }
                }
                await db.SaveChangesAsync();
                if (listEvents == null) { return Unauthorized("User is not signed in!"); }
                return Ok(listLocalEvents);
            }
            catch
            {
                return Unauthorized();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpGet("task")]
        public async Task<IActionResult> RetrieveTasks()
        {
            var email = HttpContext.User?.FindFirst(ClaimTypes.Email).Value;
            var tasks = db.Tasks
                .Where(x => x.Email == email)
                .ToList();
            return Ok(tasks);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("add_task")]
        public async Task<IActionResult> AddTask([FromBody] CalendarDashboard.Models.Task task)
        {
            db.Tasks.Add(task);
            db.SaveChangesAsync();
            return Ok(task);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("delete_task")]
        public async Task<IActionResult> RemoveTask([FromBody] int taskId)
        {
            var task = await db.Tasks.FindAsync(taskId);
            db.Tasks.Remove(task);
            db.SaveChangesAsync();
            return Ok(task);
        }


        [ValidateAntiForgeryToken]
        [HttpPost("add_event")]
        public async Task<IActionResult> AddEvent([FromBody] EventDTO localEvent)
        {
            try
            {
                var calendarService = new GoogleCalendarService(calendarServiceHandler, tokenServiceHandler, httpContextAccessor);
                var startTime = new EventDateTime()
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(localEvent.StartTime)
                };
                var endTime = new EventDateTime()
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(localEvent.EndTime)
                };
                var response = await calendarService.AddEvent(localEvent.Name, localEvent.Description, startTime, endTime, localEvent.Attendees);
                var newEvent = new LocalEvent()
                {
                    Name = response!.Summary,
                    Description = response.Description,
                    StartTime = response.Start,
                    EndTime = response.End,
                    EventId = response.Id,
                };
                return Ok(newEvent);
            }
            catch
            {
               return Unauthorized();
            }

        }

        [ValidateAntiForgeryToken]
        [HttpPost("delete_event")]
        public async Task<IActionResult> deleteEvent([FromBody] string eventId)
        {
            try
            {
                var calendarService = new GoogleCalendarService(calendarServiceHandler, tokenServiceHandler, httpContextAccessor);
                await calendarService.DeleteEvent(eventId);
                return Ok("Event " + eventId + " has been deleted");

            }
            catch
            {
                return Unauthorized();
            }

        }

        //[ValidateAntiForgeryToken]
        //[HttpPost("refresh")]
        //public async Task<IActionResult> RefreshToken()
        //{
        //    var refreshToken = await tokenServiceHandler.GetDecryptedRefreshToken();
        //    if(refreshToken == null)
        //    {
        //        return Unauthorized();
        //    }
        //    await tokenServiceHandler.RefreshAccessToken(refreshToken!);
        //    return Ok();

        //}
    }
}
