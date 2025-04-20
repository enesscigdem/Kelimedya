using Microsoft.AspNetCore.Mvc;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Authorization;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReportsController : Controller
    {
        private readonly HttpClient _httpClient;
        public ReportsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }
        
        // GET: /Admin/Reports
        public async Task<IActionResult> Index()
        {
            var reports = await _httpClient.GetFromJsonAsync<List<ReportViewModel>>("api/reports");
            return View(reports);
        }

        // GET: /Admin/Reports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Reports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var response = await _httpClient.PostAsJsonAsync("api/reports", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Rapor başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Reports/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var report = await _httpClient.GetFromJsonAsync<ReportViewModel>($"api/reports/{id}");
            if (report == null)
                return NotFound();
            return View(report);
        }

        // POST: /Admin/Reports/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportViewModel model)
        {
            if (id != model.Id)
                return BadRequest();
            if (!ModelState.IsValid)
                return View(model);
            var response = await _httpClient.PutAsJsonAsync($"api/reports/{id}", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Rapor başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Reports/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/reports/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Reports deleted successfully.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");

        }
    }
}
