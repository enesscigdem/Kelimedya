using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.WebApp.Areas.Student.Models;

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
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(StudentProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _factory.CreateClient("DefaultApi");
            var response = await client.PutAsJsonAsync($"api/users/{model.Id}", new
            {
                id = model.Id,
                userName = model.UserName,
                email = model.Email,
                name = model.Name,
                surname = model.Surname,
                phoneNumber = model.PhoneNumber,
                classGrade = model.ClassGrade,
                role = "Student",
                teacherId = (int?)null
            });

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