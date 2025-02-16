using Microsoft.AspNetCore.Mvc;
using Paragraph.WebApp.Areas.Admin.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;

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
        
        // GET: /Admin/Lessons
        // Artık courseId parametresi olmadan tüm dersleri listeliyoruz.
        public async Task<IActionResult> Index()
        {
            var lessons = await _httpClient.GetFromJsonAsync<List<LessonViewModel>>("api/lessons");
            return View(lessons);
        }

        // GET: /Admin/Lessons/Create?courseId=...
        // Ders oluştururken ilgili courseId gönderilebilir.
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

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.CourseId.ToString()), "CourseId");
                content.Add(new StringContent(model.Title), "Title");
                content.Add(new StringContent(model.Description ?? ""), "Description");
                content.Add(new StringContent(model.SequenceNo.ToString()), "SequenceNo");

                if (model.ImageFile != null)
                {
                    byte[] fileBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageFile.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    content.Add(fileContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await _httpClient.PostAsync("api/lessons", content);
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

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Id.ToString()), "Id");
                content.Add(new StringContent(model.CourseId.ToString()), "CourseId");
                content.Add(new StringContent(model.Title), "Title");
                content.Add(new StringContent(model.Description ?? ""), "Description");
                content.Add(new StringContent(model.SequenceNo.ToString()), "SequenceNo");

                if (model.ImageFile != null)
                {
                    byte[] fileBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageFile.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                    content.Add(fileContent, "ImageFile", model.ImageFile.FileName);
                }

                var response = await _httpClient.PutAsync($"api/lessons/{id}", content);
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
        }

        // GET: /Admin/Lessons/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/lessons/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Ders başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index", "Courses", new { area = "Admin" });
        }
    }
}
