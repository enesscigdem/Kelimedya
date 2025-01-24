using Microsoft.AspNetCore.Mvc;

namespace Paragraph.WebApp.Areas.Student.Controllers
{
    [Area("Student")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MyCoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}