using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GamesController : Controller
    {
        private readonly HttpClient _httpClient;
        public GamesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // GET: /Admin/Games
        public async Task<IActionResult> Index()
        {
            var games = await _httpClient.GetFromJsonAsync<List<GameViewModel>>("api/games");
            return View(games ?? new List<GameViewModel>());
        }

        // GET: /Admin/Games/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/games", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Oyun başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            return View(model);
        }

        // GET: /Admin/Games/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var game = await _httpClient.GetFromJsonAsync<GameViewModel>($"api/games/{id}");
            if (game == null)
                return NotFound();
            return View(game);
        }

        // POST: /Admin/Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GameViewModel model)
        {
            if (id != model.Id)
                return BadRequest();
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PutAsJsonAsync($"api/games/{id}", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Oyun başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            return View(model);
        }

        // GET: /Admin/Games/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/games/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Oyun başarıyla silindi.";
            else
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
