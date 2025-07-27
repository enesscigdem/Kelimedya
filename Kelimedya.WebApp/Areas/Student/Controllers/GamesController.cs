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

            try
            {
                var games = await _httpClient.GetFromJsonAsync<List<Game>>("api/games");
                var stats = await _httpClient.GetFromJsonAsync<List<StudentGameStatistic>>($"api/gamestats/{studentId}");

                ViewBag.TotalGames = games?.Count ?? 0;
                ViewBag.CompletedGames = stats?.Count ?? 0;
                ViewBag.TotalScore = stats?.Sum(s => s.Score) ?? 0;
                ViewBag.TotalMinutes = stats != null ? (int)(stats.Sum(s => s.DurationSeconds) / 60) : 0;
            }
            catch (HttpRequestException)
            {
                ViewBag.TotalGames = 0;
                ViewBag.CompletedGames = 0;
                ViewBag.TotalScore = 0;
                ViewBag.TotalMinutes = 0;
            }

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

        public async Task<IActionResult> WordImage()
        {
            ViewData["GameId"] = await GetGameIdAsync("Kelimeden Görsel");
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

        public async Task<IActionResult> SynonymMatch()
        {
            ViewData["GameId"] = await GetGameIdAsync("Eş Yakın Eşleştirme");
            return View();
        }

        public async Task<IActionResult> SentenceBuilder()
        {
            ViewData["GameId"] = await GetGameIdAsync("Cümle Kurma");
            return View();
        }
    }
}