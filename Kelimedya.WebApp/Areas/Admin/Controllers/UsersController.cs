using Microsoft.AspNetCore.Mvc;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsersController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _httpClient.GetFromJsonAsync<List<UserViewModel>>("api/users");
            return View(users);
        }

        // GET: /Admin/Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Map ViewModel’i API’ye göndereceğimiz DTO’ya dönüştürün
            var dto = new
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                Name = model.Name,
                Surname = model.Surname,
                Role = model.Role
            };

            var response = await _httpClient.PostAsJsonAsync("api/users", dto);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
                return View(model);
            }
        }

        // GET: /Admin/Users/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var response = await _httpClient.GetAsync($"api/users/{id}");
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserViewModel>();
                if (user == null)
                {
                    return NotFound();
                }

                // FullName’i parçalayıp Name ve Surname değerlerini oluşturuyoruz
                var nameParts = user.FullName.Split(' ');
                var model = new EditUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = nameParts[0],
                    Surname = nameParts.Length > 1 ? string.Join(" ", nameParts, 1, nameParts.Length - 1) : "",
                    Role = user.Role
                };
                return View(model);
            }

            return NotFound();
        }

        // POST: /Admin/Users/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Map ViewModel’i güncelleme DTO’suna dönüştürün
            var dto = new
            {
                Id = model.Id,
                UserName = model.UserName,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                Role = model.Role
            };

            var response = await _httpClient.PutAsJsonAsync($"api/users/{model.Id}", dto);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
                return View(model);
            }
        }

        // GET: /Admin/Users/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Kullanıcı başarıyla silindi.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }

            return RedirectToAction("Index");
        }
    }
}