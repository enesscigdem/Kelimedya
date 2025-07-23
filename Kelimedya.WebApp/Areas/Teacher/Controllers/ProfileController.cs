using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Kelimedya.WebApp.Areas.Teacher.Models;

namespace Kelimedya.WebApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = RoleNames.Teacher)]
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
            var model = await client.GetFromJsonAsync<TeacherProfileViewModel>($"api/users/{userId}")
                         ?? new TeacherProfileViewModel { Id = userId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TeacherProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _factory.CreateClient("DefaultApi");
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.Id), "Id");
            form.Add(new StringContent(model.UserName), "UserName");
            form.Add(new StringContent(model.Email), "Email");
            form.Add(new StringContent(model.Name ?? string.Empty), "Name");
            form.Add(new StringContent(model.Surname ?? string.Empty), "Surname");
            form.Add(new StringContent(model.PhoneNumber ?? string.Empty), "PhoneNumber");
            form.Add(new StringContent("Teacher"), "Role");
            form.Add(new StringContent(string.Empty), "TeacherId");
            form.Add(new StringContent(string.Empty), "ClassGrade");
            if (model.ImageFile != null)
            {
                var stream = new StreamContent(model.ImageFile.OpenReadStream());
                stream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ImageFile.ContentType);
                form.Add(stream, "ImageFile", model.ImageFile.FileName);
            }

            var response = await client.PutAsync($"api/users/{model.Id}", form);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
                return View(model);
            }

            var updated = await client.GetFromJsonAsync<TeacherProfileViewModel>($"api/users/{model.Id}") ?? model;
            ViewData["Success"] = "Profil güncellendi";
            return View(updated);
        }
    }
}