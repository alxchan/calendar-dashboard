using System.Security.Claims;
using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public CalendarAPIController(CalendarDBContext db, CalendarServiceHandler calendarServiceHandler, TokenServiceHandler tokenServiceHandler, IHttpContextAccessor httpContext)
        {
            this.db = db;
            this.calendarServiceHandler = calendarServiceHandler;
            this.tokenServiceHandler = tokenServiceHandler;
            this.httpContextAccessor = httpContext;
        }

        [HttpGet("test")]
        public async Task<IActionResult> RetrieveUpcomingEvents()
        {
            Console.WriteLine("Service: " + HttpContext.User?.FindFirst("service")!.Value);
            var calendarService = new GoogleCalendarService(calendarServiceHandler, tokenServiceHandler, httpContextAccessor);
            var email = HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value;
            var calendarIds = await calendarService.GetCalendarIds();
            for (int i = 0; i < calendarIds.Count; i++) {
                Console.WriteLine(calendarIds);
            
            }
            var calendarId = calendarIds[calendarIds.Count-1];
            var listEvents = await calendarService.GetUpcomingEvents(calendarId);
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
                }
                else {
                    db.Events.Add(new LocalEvent() { Email = email!, EventId = item.Id, CalendarId = calendarId, Name = item.Summary, Description = item.Description, StartTime = item.Start!, EndTime = item.End!, Location = item.Location });

                }
                    Console.WriteLine(item.CreatedDateTimeOffset);
            }
            await db.SaveChangesAsync();
            if (listEvents == null) { return Unauthorized("User is not signed in!"); }
            return Ok(listEvents);
        }

        //[HttpGet("refresh")]
        //public async Task<string> RefreshToken() 
        //{
        //    return await tokenServiceHandler.RefreshAccessToken("");
        
        //}
    }
}
