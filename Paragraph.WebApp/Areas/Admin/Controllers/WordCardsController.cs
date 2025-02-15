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
    public class WordCardsController : Controller
    {
         private readonly HttpClient _httpClient;
         public WordCardsController(IHttpClientFactory httpClientFactory)
         {
             _httpClient = httpClientFactory.CreateClient("DefaultApi");
         }

         // GET: /Admin/WordCards?lessonId=...
         public async Task<IActionResult> Index()
         {
             var wordCards = await _httpClient.GetFromJsonAsync<List<WordCardViewModel>>("api/wordcards");
             return View(wordCards);
         }

         // GET: /Admin/WordCards/Create?lessonId=...
         public IActionResult Create(int lessonId)
         {
             ViewBag.LessonId = lessonId;
             return View();
         }

         // POST: /Admin/WordCards/Create
         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(WordCardViewModel model)
         {
             if (!ModelState.IsValid)
                 return View(model);
             var response = await _httpClient.PostAsJsonAsync("api/wordcards", model);
             if (response.IsSuccessStatusCode)
             {
                 TempData["Success"] = "Kelime kartı başarıyla oluşturuldu.";
                 return RedirectToAction("Index", "Courses", new { area = "Admin" });
             }
             else
             {
                 ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                 return View(model);
             }
         }

         // GET: /Admin/WordCards/Edit/{id}
         public async Task<IActionResult> Edit(int id)
         {
             var card = await _httpClient.GetFromJsonAsync<WordCardViewModel>($"api/wordcards/{id}");
             if (card == null)
                 return NotFound();
             return View(card);
         }

         // POST: /Admin/WordCards/Edit/{id}
         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(int id, WordCardViewModel model)
         {
             if (id != model.Id)
                 return BadRequest();
             if (!ModelState.IsValid)
                 return View(model);
             var response = await _httpClient.PutAsJsonAsync($"api/wordcards/{id}", model);
             if (response.IsSuccessStatusCode)
             {
                 TempData["Success"] = "Kelime kartı başarıyla güncellendi.";
                 return RedirectToAction("Index", "Courses", new { area = "Admin" });
             }
             else
             {
                 ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                 return View(model);
             }
         }

         // GET: /Admin/WordCards/Delete/{id}
         public async Task<IActionResult> Delete(int id)
         {
             var response = await _httpClient.DeleteAsync($"api/wordcards/{id}");
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
