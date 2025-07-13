using CalendarDashboard.Services;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Mvc;

namespace CalendarDashboard.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CalendarAPIController : Controller
    {
        private readonly GoogleCalendarService _calendarService;

        public CalendarAPIController(GoogleCalendarService calendarService) {
            _calendarService = calendarService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var events = await _calendarService.GetUpcomingEvents();
            return Ok(events);
        }
    }
}
