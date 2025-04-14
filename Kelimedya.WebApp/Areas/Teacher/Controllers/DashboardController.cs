using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.WebApp.Areas.Teacher.Models;
using System.Collections.Generic;

namespace Kelimedya.WebApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        public async Task<IActionResult> Index()
        {
            // API tarafındaki endpoint'i çağırarak dinamik veriyi çekiyoruz.
            var dashboardData = await _httpClient.GetFromJsonAsync<TeacherDashboardViewModel>("api/teacher/dashboard");
            if (dashboardData == null)
            {
                dashboardData = new TeacherDashboardViewModel
                {
                    TeacherName = "Ayşe Öğretmen",
                    TotalStudents = 0,
                    NewStudents = 0,
                    AverageProgress = 0,
                    StudentReports = new List<StudentReportViewModel>()
                };
            }
            return View(dashboardData);
        }
    }
}