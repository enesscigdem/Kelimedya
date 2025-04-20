using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Student.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kelimedya.WebApp.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.Student)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MyWordsController : Controller
    {
        private readonly HttpClient _httpClient;

        public MyWordsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        public async Task<IActionResult> Index()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var learnedWords = await _httpClient.GetFromJsonAsync<List<WordCardWithProgressViewModel>>($"api/progress/wordcards/learned/{studentId}");
            var model = new MyWordsViewModel
            {
                LearnedWords = learnedWords ?? new List<WordCardWithProgressViewModel>()
            };
            return View(model);
        }
    }
}