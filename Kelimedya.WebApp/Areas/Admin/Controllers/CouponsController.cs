using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Collections.Generic;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CouponsController : Controller
    {
        private readonly HttpClient _client;
        public CouponsController(IHttpClientFactory http) => _client = http.CreateClient("DefaultApi");

        // GET: Admin/Coupons
        public async Task<IActionResult> Index()
        {
            var list = await _client.GetFromJsonAsync<List<CouponDto>>("api/coupons");
            return View(list);
        }

        // GET: Admin/Coupons/Create
        public IActionResult Create()
            => View(new CreateCouponDto());

        // POST: Admin/Coupons/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCouponDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var resp = await _client.PostAsJsonAsync("api/coupons", dto);
            if (resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            // API’den gelen hatayı okumak da faydalı olabilir:
            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Oluşturulamadı: {error}");
            return View(dto);
        }

        // GET: Admin/Coupons/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _client.GetFromJsonAsync<CouponDto>($"api/coupons/{id}");
            if (dto == null) return NotFound();

            // CouponDto → UpdateCouponDto mapping
            var update = new UpdateCouponDto
            {
                Id = dto.Id,
                Code = dto.Code,
                Description = dto.Description,
                DiscountType = (byte)dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                CreatedAt = dto.CreatedAt,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsActive = dto.IsActive
            };
            return View(update);
        }

        // POST: Admin/Coupons/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCouponDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var resp = await _client.PutAsJsonAsync($"api/coupons/{dto.Id}", dto);
            if (resp.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Güncellenemedi: {error}");
            return View(dto);
        }

        // POST: Admin/Coupons/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var resp = await _client.DeleteAsync($"api/coupons/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "Silme işlemi başarısız.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
