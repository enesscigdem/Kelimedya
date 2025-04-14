using Microsoft.AspNetCore.Mvc;
using Kelimedya.Core.Entities;
using Kelimedya.Core.Models;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WidgetsController : Controller
    {
        private readonly HttpClient _httpClient;
        public WidgetsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // GET: /Admin/Widgets
        public async Task<IActionResult> Index()
        {
            // API'den gelen widget'lar (Widget[]) alınıyor
            var widgets = await _httpClient.GetFromJsonAsync<Widget[]>("api/widgets") 
                          ?? new Widget[0];

            // Gelen entity'leri WidgetViewModel'e map ediyoruz
            var viewModel = widgets.Select(w => new WidgetViewModel
            {
                Id = w.Id,
                Key = w.Key,
                Subject = w.Subject,
                CreatedAt = w.CreatedAt
            }).ToList();

            return View(viewModel);
        }

        // GET: /Admin/Widgets/Create
        public IActionResult Create()
        {
            return View(new Widget());
        }

        // POST: /Admin/Widgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Widget widget)
        {
            if (!ModelState.IsValid)
            {
                return View(widget);
            }

            var response = await _httpClient.PostAsJsonAsync("api/widgets", widget);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Widget başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Widget oluşturulurken hata oluştu.");
                return View(widget);
            }
        }

        // GET: /Admin/Widgets/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var widget = await _httpClient.GetFromJsonAsync<Widget>($"api/widgets/{id}");
            if (widget == null)
                return NotFound();

            return View(widget);
        }

        // POST: /Admin/Widgets/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Widget widget)
        {
            if (id != widget.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(widget);

            var response = await _httpClient.PutAsJsonAsync($"api/widgets/{id}", widget);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Widget başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Widget güncellenirken hata oluştu.");
                return View(widget);
            }
        }
        // GET: /Admin/WordCards/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/widgets/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Widget deleted successfully.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
