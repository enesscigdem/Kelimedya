using Microsoft.AspNetCore.Mvc;
using Paragraph.WebApp.Areas.Admin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Paragraph.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;
        public ProductsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // GET: /Admin/Products
        public async Task<IActionResult> Index()
        {
            var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("api/products");
            return View(products);
        }

        // GET: /Admin/Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/products", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _httpClient.GetFromJsonAsync<ProductViewModel>($"api/products/{id}");
            if (product == null)
                return NotFound();
            return View(product);
        }

        // POST: /Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
                return BadRequest();
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Product deleted successfully.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
