using Microsoft.AspNetCore.Mvc;

namespace Paragraph.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LogoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");

            return RedirectToAction("Login", "Auth");
        }
    }
}