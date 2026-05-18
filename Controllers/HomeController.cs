using CRM_Granat.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CRM_Granat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Users { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
