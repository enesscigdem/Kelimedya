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
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;
        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _httpClient.GetFromJsonAsync<List<OrderViewModel>>("api/orders");
            return View(orders);
        }

        // GET: /Admin/Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var response = await _httpClient.PostAsJsonAsync("api/orders", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Sipariş başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Orders/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _httpClient.GetFromJsonAsync<OrderViewModel>($"api/orders/{id}");
            if (order == null)
                return NotFound();
            return View(order);
        }

        // POST: /Admin/Orders/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderViewModel model)
        {
            if (id != model.Id)
                return BadRequest();
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PutAsJsonAsync($"api/orders/{id}", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Sipariş başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Orders/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _httpClient.GetFromJsonAsync<OrderViewModel>($"api/orders/{id}");
            if (order == null)
                return NotFound();
            return RedirectToAction("Index");

        }
    }
}
