using Microsoft.AspNetCore.Mvc;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Kelimedya.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Kelimedya.Core.Entities;
using System;

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

        // GET: /Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _httpClient.GetFromJsonAsync<List<OrderViewModel>>("api/orders");
            return View(orders);
        }

        // GET: /Admin/Orders/Create
        public async Task<IActionResult> Create()
        {
            var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
            ViewBag.Users = users?.Where(u => u.Role == RoleNames.User || u.Role == RoleNames.Student).ToList();
            var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("api/products");
            ViewBag.Products = products;
            return View();
        }

        // POST: /Admin/Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
                ViewBag.Users = users?.Where(u => u.Role == RoleNames.User || u.Role == RoleNames.Student).ToList();
                var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("api/products");
                ViewBag.Products = products;
                return View(model);
            }

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
                        UnitPrice = model.TotalAmount,
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
            var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
            ViewBag.Users = users?.Where(u => u.Role == RoleNames.User || u.Role == RoleNames.Student).ToList();
            var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("api/products");
            ViewBag.Products = products;
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
            {
                var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
                ViewBag.Users = users?.Where(u => u.Role == RoleNames.User || u.Role == RoleNames.Student).ToList();
                var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("api/products");
                ViewBag.Products = products;
                return View(model);
            }

            var order = new Order
            {
                Id = model.Id,
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
                        UnitPrice = model.TotalAmount,
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
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
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
