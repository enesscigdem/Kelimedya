using Microsoft.AspNetCore.Mvc;

namespace Kelimedya.WebApp.Controllers
{
    public class PolicyController : Controller
    {
        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();
        public IActionResult Cookies() => View();
        public IActionResult Kvkk() => View();
        public IActionResult DistanceSales() => View();
        public IActionResult PreInformation() => View();
        public IActionResult Refund() => View();
    }
}