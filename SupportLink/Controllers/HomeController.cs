using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportLink.Models;
using System.Diagnostics;

namespace SupportLink.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // ? You can pass user details to the view using ViewBag
            ViewBag.UserName = User.Identity?.Name;
            //ViewBag.Role = User.IsInRole("Admin") ? "Admin" : "Staff" : "Agent";
            ViewBag.Role = User.IsInRole("Admin") ? "Admin"
             : User.IsInRole("Agent") ? "Agent"
             : User.IsInRole("Staff") ? "Staff"
             : "User";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
