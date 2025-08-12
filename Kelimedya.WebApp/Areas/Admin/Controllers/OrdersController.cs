using Microsoft.AspNetCore.Mvc;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Kelimedya.Core.Models; // UserDto, ProductViewModel burada
using Microsoft.AspNetCore.Authorization;
using Kelimedya.Core.Entities; // Order, OrderItem
using System;
using System.Linq;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }
        private static string GenerateShortOrderNumber()
            => Guid.NewGuid().ToString("N")[..8].ToUpper(); // örn: 7D3A9C10

        private async Task PopulateSelectionsAsync()
        {
            var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users") ?? new List<UserDto>();
            ViewBag.Users = users
                .Where(u => u.Role == RoleNames.User || u.Role == RoleNames.Student)
                .ToList();

            var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("api/products") ?? new List<ProductViewModel>();
            ViewBag.Products = products;
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _httpClient.GetFromJsonAsync<List<OrderViewModel>>("api/orders");
            return View(orders);
        }

        // GET: /Admin/Orders/Create
        public async Task<IActionResult> Create()
        {
            await PopulateSelectionsAsync();
            var model = new OrderViewModel
            {
                OrderDate = DateTime.Now.Date,
                OrderNumber = GenerateShortOrderNumber()
            };
            return View(model);
        }


        // POST: /Admin/Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync();
                return View(model);
            }

            // Server-side güvence: boş gelirse yeniden üret
            if (string.IsNullOrWhiteSpace(model.OrderNumber))
                model.OrderNumber = GenerateShortOrderNumber();

            var order = new Order
            {
                OrderNumber = model.OrderNumber,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                OrderDate = model.OrderDate,
                CouponCode = model.CouponCode,
                DiscountAmount = model.DiscountAmount,
                SubTotal = model.SubTotal,
                TotalAmount = model.TotalAmount,
                IsActive = model.IsActive,
                IsDeleted = model.IsDeleted,
                UserId = model.UserId,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = model.ProductId,
                        Quantity = 1,
                        // Ürün birim fiyatı: SubTotal varsa onu, yoksa TotalAmount
                        UnitPrice = model.SubTotal > 0 ? model.SubTotal : model.TotalAmount,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("api/orders", order);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Sipariş başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            await PopulateSelectionsAsync();
            return View(model);
        }

        // GET: /Admin/Orders/Edit/{id}
        // GET: /Admin/Orders/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            // API'den doğrudan Order (entity) çekelim ki Items'a erişelim
            var orderEntity = await _httpClient.GetFromJsonAsync<Order>($"api/orders/{id}");
            if (orderEntity == null)
                return NotFound();

            // İlk (ve tek) kalemden ProductId'yi al
            var firstItem = orderEntity.Items?.FirstOrDefault();

            var vm = new OrderViewModel
            {
                Id = orderEntity.Id,
                OrderNumber = orderEntity.OrderNumber,
                OrderDate = orderEntity.OrderDate,
                CustomerName = orderEntity.CustomerName,
                CustomerEmail = orderEntity.CustomerEmail,
                CouponCode = orderEntity.CouponCode,
                DiscountAmount = orderEntity.DiscountAmount,
                SubTotal = orderEntity.SubTotal,
                TotalAmount = orderEntity.TotalAmount,
                IsActive = orderEntity.IsActive,
                IsDeleted = orderEntity.IsDeleted,
                UserId = orderEntity.UserId,
                Status = orderEntity.Status,
                ProductId = firstItem?.ProductId ?? 0   // <<< kritik satır
            };

            await PopulateSelectionsAsync();
            return View(vm);
        }


        // POST: /Admin/Orders/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync();
                return View(model);
            }

            var order = new Order
            {
                Id = model.Id,
                OrderNumber = model.OrderNumber, // readonly ama server'a da gidiyor
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                OrderDate = model.OrderDate,
                CouponCode = model.CouponCode,
                DiscountAmount = model.DiscountAmount,
                SubTotal = model.SubTotal,
                TotalAmount = model.TotalAmount,
                IsActive = model.IsActive,
                IsDeleted = model.IsDeleted,
                UserId = model.UserId,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow,   // (İsteğe bağlı: API tarafı set edebilir)
                ModifiedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = model.ProductId,
                        Quantity = 1,
                        UnitPrice = model.SubTotal > 0 ? model.SubTotal : model.TotalAmount,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    }
                }
            };

            var response = await _httpClient.PutAsJsonAsync($"api/orders/{id}", order);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Sipariş başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            await PopulateSelectionsAsync();
            return View(model);
        }

        // DELETE: /Admin/Orders/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/orders/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Sipariş başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
