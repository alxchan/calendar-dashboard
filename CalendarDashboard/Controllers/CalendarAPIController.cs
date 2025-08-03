using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
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
        private readonly IHttpContextAccessor httpContextAccessor;

        public CalendarAPIController(CalendarDBContext db, CalendarServiceHandler calendarServiceHandler, TokenServiceHandler tokenServiceHandler, IHttpContextAccessor httpContext)
        {
            this.db = db;
            this.calendarServiceHandler = calendarServiceHandler;
            this.tokenServiceHandler = tokenServiceHandler;
            this.httpContextAccessor = httpContext;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var calendarService = new GoogleCalendarService(calendarServiceHandler, tokenServiceHandler, httpContextAccessor);
            var listEvents = await calendarService.GetUpcomingEvents();
            if (listEvents == null) { return Unauthorized("User is not signed in!"); }
            return Ok(listEvents);
        }

        [HttpGet("refresh")]
        public async Task<string> RefreshToken() 
        {
            return await tokenServiceHandler.RefreshAccessToken("");
        
        }
    }
}
