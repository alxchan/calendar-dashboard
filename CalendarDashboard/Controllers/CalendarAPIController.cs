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
            var events = await calendarService.AddEvent();
            if (events == null) { return Unauthorized("User is not signed in!"); }
            return Ok(events);
        }

        [HttpGet("refresh")]
        public async Task<string> RefreshToken() 
        {
            return await tokenServiceHandler.RefreshAccessToken("");
        
        }
    }
}
