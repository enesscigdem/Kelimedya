using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Kelimedya.WebApp.Areas.Student.Models;
using Kelimedya.WebApp.Models;

namespace Kelimedya.WebApp.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.Student)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public ProfileController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _factory.CreateClient("DefaultApi");
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var model = await client.GetFromJsonAsync<StudentProfileViewModel>($"api/users/{userId}")
                        ?? new StudentProfileViewModel { Id = userId };

            try
            {
                var score = await client.GetFromJsonAsync<ScoreInfoViewModel>($"api/gamestats/score/{userId}");
                var learned = await client.GetFromJsonAsync<List<object>>($"api/progress/wordcards/learned/{userId}");
                ViewBag.TotalScore = score?.TotalScore ?? 0;
                ViewBag.LearnedWords = learned?.Count ?? 0;
            }
            catch (HttpRequestException)
            {
                ViewBag.TotalScore = 0;
                ViewBag.LearnedWords = 0;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(StudentProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _factory.CreateClient("DefaultApi");
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Id), "Id");
            content.Add(new StringContent(model.UserName), "UserName");
            content.Add(new StringContent(model.Email), "Email");
            content.Add(new StringContent(model.Name ?? string.Empty), "Name");
            content.Add(new StringContent(model.Surname ?? string.Empty), "Surname");
            content.Add(new StringContent(model.PhoneNumber ?? string.Empty), "PhoneNumber");
            if (model.ClassGrade.HasValue)
                content.Add(new StringContent(model.ClassGrade.Value.ToString()), "ClassGrade");
            content.Add(new StringContent("Student"), "Role");
            content.Add(new StringContent(string.Empty), "TeacherId");
            if (model.ImageFile != null)
            {
                var stream = new StreamContent(model.ImageFile.OpenReadStream());
                stream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ImageFile.ContentType);
                content.Add(stream, "ImageFile", model.ImageFile.FileName);
            }

            var response = await client.PutAsync($"api/users/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
                return View(model);
            }

            var updated = await client.GetFromJsonAsync<StudentProfileViewModel>($"api/users/{model.Id}")
                          ?? model;

            ViewData["Success"] = "Profil güncellendi";
            return View(updated);
        }
    }
}