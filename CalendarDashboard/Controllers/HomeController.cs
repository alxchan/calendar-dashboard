using System.Diagnostics;
using CalendarDashboard.Services;
using Google.Apis.Auth.OAuth2;
using CalendarDashboard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;

namespace CalendarDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAntiforgery antiforgery;

        public HomeController(ILogger<HomeController> logger, IAntiforgery antiforgery)
        {
            _logger = logger;
            this.antiforgery = antiforgery;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Calendar() 
        {
            //ViewData["CSRF-Token"] = antiforgery.GetAndStoreTokens(HttpContext!).RequestToken;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
