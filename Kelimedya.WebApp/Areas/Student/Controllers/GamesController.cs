using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Entities;
using System.Linq;
using System.Security.Claims;
using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.Student)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GamesController : Controller
    {
        private readonly HttpClient _httpClient;

        public GamesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        private async Task<int> GetGameIdAsync(string title)
        {
            var games = await _httpClient.GetFromJsonAsync<List<Game>>("api/games");
            return games?.FirstOrDefault(g => string.Equals(g.Title, title, System.StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool hasCourses;
            try
            {
                var courses = await _httpClient.GetFromJsonAsync<List<CourseViewModel>>($"api/orders/courses/{studentId}");
                hasCourses = courses != null && courses.Any();
            }
            catch (HttpRequestException)
            {
                hasCourses = false;
            }
            ViewBag.HasCourses = hasCourses;
            return View();
        }

        public async Task<IActionResult> AdamAsmaca()
        {
            ViewData["GameId"] = await GetGameIdAsync("Adam Asmaca");
            return View();
        }

        public async Task<IActionResult> KelimeBulmaca()
        {
            ViewData["GameId"] = await GetGameIdAsync("Kelime Bulmaca");
            return View();
        }

        public async Task<IActionResult> VisualPrompt()
        {
            ViewData["GameId"] = await GetGameIdAsync("Görselden Soru");
            return View();
        }

        public async Task<IActionResult> BubbleLetters()
        {
            ViewData["GameId"] = await GetGameIdAsync("Kabarcık Harfler");
            return View();
        }

        public async Task<IActionResult> CrossPuzzle()
        {
            ViewData["GameId"] = await GetGameIdAsync("Çengel Bulmaca");
            return View();
        }

        public async Task<IActionResult> FillBlanks()
        {
            ViewData["GameId"] = await GetGameIdAsync("Boşluk Doldurma");
            return View();
        }
    }
}