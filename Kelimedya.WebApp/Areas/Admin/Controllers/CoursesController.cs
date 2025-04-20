using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using System.Linq;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;
using Kelimedya.WebApp.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
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
            var courses = await _httpClient.GetFromJsonAsync<List<CourseAggregateViewModel>>("api/courses");
            if (courses == null)
            {
                courses = new List<CourseAggregateViewModel>();
            }

            // Her kurs için ilgili dersleri ve kelime kartlarını da yüklemek için:
            foreach (var course in courses)
            {
                var lessons = await _httpClient.GetFromJsonAsync<List<LessonAggregateViewModel>>($"api/lessons/bycourse/{course.Id}");
                if (lessons == null)
                {
                    lessons = new List<LessonAggregateViewModel>();
                }

                foreach (var lesson in lessons)
                {
                    // Tüm kelime kartlarını çekip, ilgili derse ait olanları filtreleyelim.
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

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Title), "Title");
                content.Add(new StringContent(model.Description ?? string.Empty), "Description");
                content.Add(new StringContent(model.LessonCount.ToString()), "LessonCount");
                content.Add(new StringContent(model.WordCount.ToString()), "WordCount");
                content.Add(new StringContent(model.IsActive.ToString()), "IsActive");

                if (model.ImageFile != null)
                {
                    var streamContent = new StreamContent(model.ImageFile.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ImageFile.ContentType);
                    content.Add(streamContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await _httpClient.PostAsync("api/courses", content);
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

            // Multipart form-data olarak gönderim yapmak için:
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Id.ToString()), "Id");
                content.Add(new StringContent(model.Title), "Title");
                content.Add(new StringContent(model.Description ?? string.Empty), "Description");
                content.Add(new StringContent(model.LessonCount.ToString()), "LessonCount");
                content.Add(new StringContent(model.WordCount.ToString()), "WordCount");
                content.Add(new StringContent(model.IsActive.ToString()), "IsActive");

                if (model.ImageFile != null)
                {
                    var streamContent = new StreamContent(model.ImageFile.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ImageFile.ContentType);
                    content.Add(streamContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await _httpClient.PutAsync($"api/courses/{id}", content);
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
        }

        // GET: /Admin/Courses/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Eğitim başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
