    using Microsoft.AspNetCore.Mvc;

    namespace Paragraph.WebApp.Areas.Teacher.Controllers
    {
        [Area("Teacher")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public class ReportsController : Controller
        {
            public IActionResult Index()
            {
                return View();
            }
            public IActionResult PerformanceReports()
            {
                return View();
            }
        }
    }