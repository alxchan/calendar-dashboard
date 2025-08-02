using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalendarDashboard.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CalendarAPIController : Controller
    {
        private readonly CalendarDBContext db;
        private readonly CalendarServiceHandler calendarServiceHandler;
        private readonly TokenServiceHandler tokenServiceHandler;

        public CalendarAPIController(CalendarDBContext db, CalendarServiceHandler calendarServiceHandler, TokenServiceHandler tokenServiceHandler) {
            this.db = db;
            this.calendarServiceHandler = calendarServiceHandler;
            this.tokenServiceHandler = tokenServiceHandler;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var calendarService = new GoogleCalendarService(calendarServiceHandler, tokenServiceHandler);
            var events = await calendarService.GetUpcomingEvents();
            if (events == null) { return Unauthorized("User is not signed in!"); }
            return Ok(events);
        }
    }
}
