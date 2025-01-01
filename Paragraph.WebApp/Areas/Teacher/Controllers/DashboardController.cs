using Microsoft.AspNetCore.Mvc;

namespace Paragraph.WebApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}