using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Teacher.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kelimedya.WebApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = RoleNames.Teacher)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReportsController : Controller
    {
        private readonly HttpClient _http;
        public ReportsController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("DefaultApi"); // BaseAddress API'yi göstermeli
        }

        // Sayfa: Öğrenci Performans Takip (overview + öğrenciler)
        public async Task<IActionResult> StudentReports(string? studentId)
        {
            var students = await _http.GetFromJsonAsync<List<StudentReportViewModel>>(
                "api/teacher/reports/students") ?? new();

            var overview = await _http.GetFromJsonAsync<TeacherOverviewViewModel>(
                "api/teacher/reports/overview") ?? new();

            var vm = new StudentReportsPageViewModel
            {
                Students = students,
                Overview = overview
            };

            ViewData["SelectedStudentId"] = studentId;
            return View(vm);
        }

        // AJAX: Karne
        [HttpGet]
        public async Task<IActionResult> Transcript(string studentId)
        {
            var dto = await _http.GetFromJsonAsync<object>($"api/teacher/reports/transcript/{studentId}");
            return Json(dto);
        }

        // AJAX: Detaylı Rapor
        [HttpGet]
        public async Task<IActionResult> DetailedReport(string studentId)
        {
            var dto = await _http.GetFromJsonAsync<object>($"api/teacher/reports/detailed/{studentId}");
            return Json(dto);
        }

        // PDF köprüleri
        [HttpGet]
        public IActionResult DownloadTranscript(string studentId)
            => Redirect($"/api/teacher/reports/download/transcript/{studentId}");

        [HttpGet]
        public IActionResult DownloadDetailedReport(string studentId)
            => Redirect($"/api/teacher/reports/download/detailed/{studentId}");
    }
}
