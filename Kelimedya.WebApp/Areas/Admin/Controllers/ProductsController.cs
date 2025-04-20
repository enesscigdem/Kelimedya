using Microsoft.AspNetCore.Mvc;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Authorization;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
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
        public async Task<IActionResult> Create()
        {
            var courses = await _httpClient.GetFromJsonAsync<List<CourseViewModel>>("api/courses");
            var model = new ProductViewModel
            {
                AvailableCourses = courses ?? new List<CourseViewModel>()
            };
            return View(model);
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Name), "Name");
                content.Add(new StringContent(model.Description ?? string.Empty), "Description");
                content.Add(new StringContent(model.Price.ToString()), "Price");

                if(model.SelectedCourseIds != null && model.SelectedCourseIds.Count > 0)
                {
                    // Örneğin: "1,2,3"
                    string courseIdsStr = string.Join(",", model.SelectedCourseIds);
                    content.Add(new StringContent(courseIdsStr), "CourseIds");
                }

                if(model.ImageFile != null)
                {
                    var streamContent = new StreamContent(model.ImageFile.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    content.Add(streamContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await _httpClient.PostAsync("api/products", content);
                if(response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                    return View(model);
                }
            }
        }

        // GET: /Admin/Products/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _httpClient.GetFromJsonAsync<ProductViewModel>($"api/products/{id}");
            if (product == null)
                return NotFound();

            // Ürünle ilişkilendirilen kursları da doldurabilmek için ek API çağrısı yapabilirsiniz.
            // Örneğin AvailableCourses'i de doldurmak için:
            var courses = await _httpClient.GetFromJsonAsync<List<CourseViewModel>>("api/courses");
            product.AvailableCourses = courses ?? new List<CourseViewModel>();

            // Eğer ürünün mevcut kurs ilişkileri varsa, SelectedCourseIds'yi de set edin:
            if(product.Courses != null)
            {
                product.SelectedCourseIds = product.Courses.Select(c => c.Id).ToList();
            }
            
            return View(product);
        }

        // POST: /Admin/Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Id.ToString()), "Id");
                content.Add(new StringContent(model.Name), "Name");
                content.Add(new StringContent(model.Description ?? string.Empty), "Description");
                content.Add(new StringContent(model.Price.ToString()), "Price");

                if(model.SelectedCourseIds != null && model.SelectedCourseIds.Count > 0)
                {
                    string courseIdsStr = string.Join(",", model.SelectedCourseIds);
                    content.Add(new StringContent(courseIdsStr), "CourseIds");
                }

                if(model.ImageFile != null)
                {
                    var streamContent = new StreamContent(model.ImageFile.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    content.Add(streamContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await _httpClient.PutAsync($"api/products/{model.Id}", content);
                if(response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                    return View(model);
                }
            }
        }

        // DELETE: /Admin/Products/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            if(response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Ürün başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
