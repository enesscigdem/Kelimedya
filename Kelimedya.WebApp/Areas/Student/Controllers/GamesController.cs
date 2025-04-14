using Microsoft.AspNetCore.Mvc;

namespace Kelimedya.WebApp.Areas.Student.Controllers
{
    [Area("Student")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GamesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AdamAsmaca()
        {
            return View();
        }
        public IActionResult KelimeBulmaca()
        {
            return View();
        }
    }
}