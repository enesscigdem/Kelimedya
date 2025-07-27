using Microsoft.AspNetCore.Mvc;
using Kelimedya.WebApp.Areas.Admin.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using Kelimedya.Core.Enum;
using Microsoft.AspNetCore.Authorization;

namespace Kelimedya.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleNames.Admin)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WordCardsController : Controller
    {
        private readonly HttpClient _httpClient;
        public WordCardsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }
        
        // GET: /Admin/WordCards
        // Tüm kelime kartlarını listeliyoruz.
        public async Task<IActionResult> Index()
        {
            var wordCards = await _httpClient.GetFromJsonAsync<List<WordCardViewModel>>("api/wordcards");
            return View(wordCards);
        }
        
        // GET: /Admin/WordCards/Create?lessonId=...
        public async Task<IActionResult> Create(int lessonId)
        {
            var games = await _httpClient.GetFromJsonAsync<List<GameViewModel>>("api/games");
            var model = new WordCardViewModel
            {
                LessonId = lessonId,
                GameQuestions = games?.Select(g => new GameQuestionViewModel
                {
                    GameId = g.Id,
                    GameTitle = g.Title
                }).ToList() ?? new List<GameQuestionViewModel>(),
                TestQuestions = new List<TestQuestionViewModel> { new(), new() }
            };

            ViewBag.LessonId = lessonId;
            return View(model);
        }
        
        // POST: /Admin/WordCards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WordCardViewModel model)
        {
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.LessonId.ToString()), "LessonId");
                content.Add(new StringContent(model.Word), "Word");
                content.Add(new StringContent(model.Synonym), "Synonym");
                content.Add(new StringContent(model.Definition), "Definition");
                content.Add(new StringContent(model.ExampleSentence ?? ""), "ExampleSentence");

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

                if (model.GameQuestions != null)
                {
                    for (int i = 0; i < model.GameQuestions.Count; i++)
                    {
                        var q = model.GameQuestions[i];
                        content.Add(new StringContent(q.GameId.ToString()), $"GameQuestions[{i}].GameId");
                        content.Add(new StringContent(q.QuestionText ?? string.Empty), $"GameQuestions[{i}].QuestionText");
                        content.Add(new StringContent(q.AnswerText ?? string.Empty), $"GameQuestions[{i}].AnswerText");
                        content.Add(new StringContent(q.ImageUrl ?? string.Empty), $"GameQuestions[{i}].ImageUrl");
                        content.Add(new StringContent(q.ImageUrl2 ?? string.Empty), $"GameQuestions[{i}].ImageUrl2");
                        content.Add(new StringContent(q.ImageUrl3 ?? string.Empty), $"GameQuestions[{i}].ImageUrl3");
                        content.Add(new StringContent(q.ImageUrl4 ?? string.Empty), $"GameQuestions[{i}].ImageUrl4");
                        content.Add(new StringContent(q.OptionA ?? string.Empty), $"GameQuestions[{i}].OptionA");
                        content.Add(new StringContent(q.OptionB ?? string.Empty), $"GameQuestions[{i}].OptionB");
                        content.Add(new StringContent(q.OptionC ?? string.Empty), $"GameQuestions[{i}].OptionC");
                        content.Add(new StringContent(q.OptionD ?? string.Empty), $"GameQuestions[{i}].OptionD");
                        content.Add(new StringContent(q.CorrectOption?.ToString() ?? string.Empty), $"GameQuestions[{i}].CorrectOption");
                        content.Add(new StringContent(q.QuestionType ?? string.Empty), $"GameQuestions[{i}].QuestionType");
                    }
                }

                if (model.TestQuestions != null)
                {
                    for (int i = 0; i < model.TestQuestions.Count; i++)
                    {
                        var t = model.TestQuestions[i];
                        content.Add(new StringContent(t.QuestionText ?? string.Empty), $"TestQuestions[{i}].QuestionText");
                        content.Add(new StringContent(t.OptionA ?? string.Empty), $"TestQuestions[{i}].OptionA");
                        content.Add(new StringContent(t.OptionB ?? string.Empty), $"TestQuestions[{i}].OptionB");
                        content.Add(new StringContent(t.OptionC ?? string.Empty), $"TestQuestions[{i}].OptionC");
                        content.Add(new StringContent(t.OptionD ?? string.Empty), $"TestQuestions[{i}].OptionD");
                        content.Add(new StringContent(t.CorrectOption.ToString()), $"TestQuestions[{i}].CorrectOption");
                    }
                }


                var response = await _httpClient.PostAsync("api/wordcards", content);
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
        }
        
        // GET: /Admin/WordCards/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var card = await _httpClient.GetFromJsonAsync<WordCardViewModel>($"api/wordcards/{id}");
            if (card == null)
                return NotFound();

            var games = await _httpClient.GetFromJsonAsync<List<GameViewModel>>("api/games");
            List<GameQuestionViewModel>? questions = null;
            List<TestQuestionViewModel>? testQs = null;
            try
            {
                questions = await _httpClient.GetFromJsonAsync<List<GameQuestionViewModel>>($"api/wordcards/{id}/questions");
                testQs = await _httpClient.GetFromJsonAsync<List<TestQuestionViewModel>>($"api/wordcards/{id}/testquestions");
            }
            catch (HttpRequestException)
            {
                questions = new List<GameQuestionViewModel>();
                testQs = new List<TestQuestionViewModel>();
            }

            card.GameQuestions = games?.Select(g =>
            {
                var q = questions?.FirstOrDefault(x => x.GameId == g.Id) ?? new GameQuestionViewModel { GameId = g.Id };
                q.GameTitle = g.Title;
                return q;
            }).ToList() ?? new List<GameQuestionViewModel>();

            card.TestQuestions = testQs != null && testQs.Count > 0
                ? testQs
                : new List<TestQuestionViewModel> { new(), new() };

            return View(card);
        }
        
        // POST: /Admin/WordCards/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WordCardViewModel model)
        {
            if (id != model.Id)
                return BadRequest();
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Id.ToString()), "Id");
                content.Add(new StringContent(model.LessonId.ToString()), "LessonId");
                content.Add(new StringContent(model.Word), "Word");
                content.Add(new StringContent(model.Synonym), "Synonym");
                content.Add(new StringContent(model.Definition), "Definition");
                content.Add(new StringContent(model.ExampleSentence ?? ""), "ExampleSentence");

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

                if (model.GameQuestions != null)
                {
                    for (int i = 0; i < model.GameQuestions.Count; i++)
                    {
                        var q = model.GameQuestions[i];
                        content.Add(new StringContent(q.GameId.ToString()), $"GameQuestions[{i}].GameId");
                        content.Add(new StringContent(q.QuestionText ?? string.Empty), $"GameQuestions[{i}].QuestionText");
                        content.Add(new StringContent(q.AnswerText ?? string.Empty), $"GameQuestions[{i}].AnswerText");
                        content.Add(new StringContent(q.ImageUrl ?? string.Empty), $"GameQuestions[{i}].ImageUrl");
                        content.Add(new StringContent(q.ImageUrl2 ?? string.Empty), $"GameQuestions[{i}].ImageUrl2");
                        content.Add(new StringContent(q.ImageUrl3 ?? string.Empty), $"GameQuestions[{i}].ImageUrl3");
                        content.Add(new StringContent(q.ImageUrl4 ?? string.Empty), $"GameQuestions[{i}].ImageUrl4");
                        content.Add(new StringContent(q.OptionA ?? string.Empty), $"GameQuestions[{i}].OptionA");
                        content.Add(new StringContent(q.OptionB ?? string.Empty), $"GameQuestions[{i}].OptionB");
                        content.Add(new StringContent(q.OptionC ?? string.Empty), $"GameQuestions[{i}].OptionC");
                        content.Add(new StringContent(q.OptionD ?? string.Empty), $"GameQuestions[{i}].OptionD");
                        content.Add(new StringContent(q.CorrectOption?.ToString() ?? string.Empty), $"GameQuestions[{i}].CorrectOption");
                        content.Add(new StringContent(q.QuestionType ?? string.Empty), $"GameQuestions[{i}].QuestionType");
                    }
                }
                if (model.TestQuestions != null)
                {
                    for (int i = 0; i < model.TestQuestions.Count; i++)
                    {
                        var t = model.TestQuestions[i];
                        content.Add(new StringContent(t.QuestionText ?? string.Empty), $"TestQuestions[{i}].QuestionText");
                        content.Add(new StringContent(t.OptionA ?? string.Empty), $"TestQuestions[{i}].OptionA");
                        content.Add(new StringContent(t.OptionB ?? string.Empty), $"TestQuestions[{i}].OptionB");
                        content.Add(new StringContent(t.OptionC ?? string.Empty), $"TestQuestions[{i}].OptionC");
                        content.Add(new StringContent(t.OptionD ?? string.Empty), $"TestQuestions[{i}].OptionD");
                        content.Add(new StringContent(t.CorrectOption.ToString()), $"TestQuestions[{i}].CorrectOption");
                    }
                }

                var response = await _httpClient.PutAsync($"api/wordcards/{id}", content);
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
        }
        
        // GET: /Admin/WordCards/Delete/{id}
        public async Task<IActionResult> Delete(int id, int lessonId)
        {
            var response = await _httpClient.DeleteAsync($"api/wordcards/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Kelime kartı başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
