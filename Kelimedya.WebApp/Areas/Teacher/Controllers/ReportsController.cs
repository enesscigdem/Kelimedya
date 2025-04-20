using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        private readonly HttpClient _httpClient;
        public ReportsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // GET: /Teacher/Reports/StudentReports
        public async Task<IActionResult> StudentReports()
        {
            var studentReports = await _httpClient.GetFromJsonAsync<List<StudentReportViewModel>>("api/teacher/reports/students");
            return View(studentReports);
        }

        // GET: /Teacher/Reports/PerformanceReports
        public async Task<IActionResult> PerformanceReports()
        {
            var performanceData = await _httpClient.GetFromJsonAsync<IEnumerable<CoursePerformanceViewModel>>("api/teacher/reports/performance");
            return View(performanceData);
        }

        // AJAX: Get Transcript (Student Report Card)
        [HttpGet]
        public async Task<IActionResult> Transcript(string studentId)
        {
            var transcript = await _httpClient.GetFromJsonAsync<TranscriptDto>($"api/teacher/reports/transcript/{studentId}");
            return Json(transcript);
        }

        // AJAX: Get Detailed Report
        [HttpGet]
        public async Task<IActionResult> DetailedReport(string studentId)
        {
            var detailedReport = await _httpClient.GetFromJsonAsync<DetailedReportDto>($"api/teacher/reports/detailed/{studentId}");
            return Json(detailedReport);
        }

        // PDF Download Redirects
        [HttpGet]
        public IActionResult DownloadTranscript(string studentId)
        {
            var downloadUrl = $"api/teacher/reports/download/transcript/{studentId}";
            return Redirect(downloadUrl);
        }

        [HttpGet]
        public IActionResult DownloadDetailedReport(string studentId)
        {
            var downloadUrl = $"api/teacher/reports/download/detailed/{studentId}";
            return Redirect(downloadUrl);
        }
    }
}
