using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Teacher.Models;
using Microsoft.AspNetCore.Authorization; // Öğrenci modelimizi içeriyor

namespace Kelimedya.WebApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = RoleNames.Teacher)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class StudentsController : Controller
    {
        private readonly HttpClient _httpClient;
        public StudentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }
        
        // Tüm öğrencileri listele
        public async Task<IActionResult> Index()
        {
            // API'den kullanıcıları çekiyoruz (API'da "api/users" endpoint’i tanımlı olmalıdır)
            var students = await _httpClient.GetFromJsonAsync<List<StudentViewModel>>("api/teacher/users");
            return View(students);
        }
    }
}