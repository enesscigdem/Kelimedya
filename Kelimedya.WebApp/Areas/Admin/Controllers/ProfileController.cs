using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
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
            var model = await client.GetFromJsonAsync<AdminProfileViewModel>($"api/users/{userId}")
                        ?? new AdminProfileViewModel { Id = userId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AdminProfileViewModel model)
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
            form.Add(new StringContent("Admin"), "Role");
            form.Add(new StringContent(string.Empty), "TeacherId");
            form.Add(new StringContent(model.PhoneNumber ?? string.Empty), "PhoneNumber");
            form.Add(new StringContent(string.Empty), "ClassGrade");
            if (model.ImageFile != null)
            {
                var stream = new StreamContent(model.ImageFile.OpenReadStream());
                stream.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                form.Add(stream, "ImageFile", model.ImageFile.FileName);
            }

            var response = await client.PutAsync($"api/users/{model.Id}", form);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) && !string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var pwdDto = new { UserId = model.Id, CurrentPassword = model.CurrentPassword, NewPassword = model.NewPassword };
                await client.PostAsJsonAsync("api/auth/change-password", pwdDto);
            }

            var updated = await client.GetFromJsonAsync<AdminProfileViewModel>($"api/users/{model.Id}") ?? model;
            ViewData["Success"] = "Profil güncellendi";
            return View(updated);
        }
    }
}