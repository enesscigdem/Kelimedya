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
    public class CoursesController : Controller
    {
        private readonly HttpClient _httpClient;
        public CoursesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }
        
        // GET: /Admin/Courses
        public async Task<IActionResult> Index()
        {
            // 1. Kursları çekiyoruz.
            var courses = await _httpClient.GetFromJsonAsync<List<CourseAggregateViewModel>>("api/courses");
            if (courses == null)
            {
                courses = new List<CourseAggregateViewModel>();
            }

            // 2. Her kurs için ilgili dersleri ve derslere ait kelime kartlarını yüklüyoruz.
            foreach (var course in courses)
            {
                // Dersleri API'den çekiyoruz (course.Id ile filtreleyerek)
                var lessons = await _httpClient.GetFromJsonAsync<List<LessonAggregateViewModel>>($"api/lessons/bycourse/{course.Id}");
                if (lessons == null)
                {
                    lessons = new List<LessonAggregateViewModel>();
                }

                // Her ders için kelime kartlarını yüklüyoruz.
                foreach (var lesson in lessons)
                {
                    // API'den tüm kelime kartlarını çekip, ilgili derse ait olanları filtreleyelim.
                    var allCards = await _httpClient.GetFromJsonAsync<List<WordCardViewModel>>("api/wordcards");
                    if (allCards != null)
                    {
                        lesson.WordCards = allCards.Where(wc => wc.LessonId == lesson.Id).ToList();
                    }
                }
                course.Lessons = lessons;
            }

            return View(courses);
        }

        // GET: /Admin/Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var response = await _httpClient.PostAsJsonAsync("api/courses", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Eğitim başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Courses/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _httpClient.GetFromJsonAsync<CourseViewModel>($"api/courses/{id}");
            if (course == null)
                return NotFound();
            return View(course);
        }

        // POST: /Admin/Courses/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseViewModel model)
        {
            if (id != model.Id)
                return BadRequest();
            if (!ModelState.IsValid)
                return View(model);
            var response = await _httpClient.PutAsJsonAsync($"api/courses/{id}", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Eğitim başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View(model);
            }
        }

        // GET: /Admin/Courses/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/courses/{id}");
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
