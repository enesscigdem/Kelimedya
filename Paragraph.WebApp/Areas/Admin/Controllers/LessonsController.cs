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
    public class LessonsController : Controller
    {
         private readonly HttpClient _httpClient;
         public LessonsController(IHttpClientFactory httpClientFactory)
         {
             _httpClient = httpClientFactory.CreateClient("DefaultApi");
         }

         // GET: /Admin/Lessons?courseId=...
         public async Task<IActionResult> Index()
         {
             var lessons = await _httpClient.GetFromJsonAsync<List<LessonViewModel>>($"api/lessons");
             return View(lessons);
         }

         // GET: /Admin/Lessons/Create?courseId=...
         public IActionResult Create(int courseId)
         {
             ViewBag.CourseId = courseId;
             return View();
         }

         // POST: /Admin/Lessons/Create
         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(LessonViewModel model)
         {
             if (!ModelState.IsValid)
                 return View(model);
             var response = await _httpClient.PostAsJsonAsync("api/lessons", model);
             if (response.IsSuccessStatusCode)
             {
                 TempData["Success"] = "Ders başarıyla oluşturuldu.";
                 return RedirectToAction("Index", "Courses", new { area = "Admin" });
             }
             else
             {
                 ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                 return View(model);
             }
         }

         // GET: /Admin/Lessons/Edit/{id}
         public async Task<IActionResult> Edit(int id)
         {
             var lesson = await _httpClient.GetFromJsonAsync<LessonViewModel>($"api/lessons/{id}");
             if (lesson == null)
                 return NotFound();
             return View(lesson);
         }

         // POST: /Admin/Lessons/Edit/{id}
         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(int id, LessonViewModel model)
         {
             if (id != model.Id)
                 return BadRequest();
             if (!ModelState.IsValid)
                 return View(model);
             var response = await _httpClient.PutAsJsonAsync($"api/lessons/{id}", model);
             if (response.IsSuccessStatusCode)
             {
                 TempData["Success"] = "Ders başarıyla güncellendi.";
                 return RedirectToAction("Index", "Courses", new { area = "Admin" });
             }
             else
             {
                 ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                 return View(model);
             }
         }

         // GET: /Admin/Lessons/Delete/{id}
         public async Task<IActionResult> Delete(int id)
         {
             var response = await _httpClient.DeleteAsync($"api/lessons/{id}");
             if (response.IsSuccessStatusCode)
             {
                 TempData["Success"] = "Product deleted successfully.";
             }
             else
             {
                 TempData["Error"] = await response.Content.ReadAsStringAsync();
             }
             return RedirectToAction("Index", "Courses", new { area = "Admin" });
         }
    }
}
