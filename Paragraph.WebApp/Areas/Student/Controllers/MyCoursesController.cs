using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Paragraph.Core.Interfaces.Business;
using Paragraph.WebApp.Areas.Admin.Models;
using Paragraph.WebApp.Areas.Student.Models;

namespace Paragraph.WebApp.Areas.Student.Controllers
{
    [Area("Student")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MyCoursesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ICurrentUserService _currentUserService;

        public MyCoursesController(IHttpClientFactory httpClientFactory, ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            _httpClient = httpClientFactory.CreateClient("DefaultApi");
        }

        // Ana sayfa: Öğrencinin aktif kurslarını ve kurs ilerlemelerini getirir.
        public async Task<IActionResult> Index()
        {
            var studentId = _currentUserService.GetUserId();

            // Aktif ve silinmemiş kursları getir
            var courses = await _httpClient.GetFromJsonAsync<List<CourseViewModel>>("api/courses/active");

            // Kurs ilerlemelerini getirirken hata alınırsa boş liste döndür
            List<StudentCourseProgressViewModel> progresses;
            try
            {
                progresses = await _httpClient.GetFromJsonAsync<List<StudentCourseProgressViewModel>>($"api/progress/courses/{studentId}");
            }
            catch (HttpRequestException)
            {
                progresses = new List<StudentCourseProgressViewModel>();
            }

            // Kursları, öğrenciye ait ilerleme bilgileriyle birleştir
            var coursesWithProgress = courses?.Select(course =>
            {
                var progress = progresses?.FirstOrDefault(p => p.CourseId == course.Id);
                return new CourseWithProgressViewModel
                {
                    Course = course,
                    Progress = progress ?? new StudentCourseProgressViewModel()
                };
            }).ToList() ?? new List<CourseWithProgressViewModel>();

            return View(coursesWithProgress);
        }

        // Belirli bir kursun detay sayfası: Kurs bilgisi, dersler ve ders ilerlemeleri.
        public async Task<IActionResult> CoursePlay(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kurs bilgilerini al
            var course = await _httpClient.GetFromJsonAsync<CourseViewModel>($"api/courses/{id}");
            if (course == null)
                return NotFound();

            // Kursa ait dersleri al
            var lessons = await _httpClient.GetFromJsonAsync<List<LessonViewModel>>($"api/lessons/bycourse/{id}");

            // Derslerin öğrenci ilerlemelerini getirirken hata alınırsa boş liste ata
            List<StudentLessonProgressViewModel> lessonProgresses;
            try
            {
                lessonProgresses = await _httpClient.GetFromJsonAsync<List<StudentLessonProgressViewModel>>($"api/progress/lessons/{studentId}/course/{id}");
            }
            catch (HttpRequestException)
            {
                lessonProgresses = new List<StudentLessonProgressViewModel>();
            }

            // ViewModel oluşturuluyor
            var viewModel = new CoursePlayViewModel
            {
                Course = course,
                Lessons = lessons?.Select(lesson =>
                {
                    var progress = lessonProgresses?.FirstOrDefault(p => p.LessonId == lesson.Id);
                    return new LessonWithProgressViewModel
                    {
                        Lesson = lesson,
                        Progress = progress ?? new StudentLessonProgressViewModel()
                    };
                }).ToList() ?? new List<LessonWithProgressViewModel>()
            };

            return View(viewModel);
        }

        // Ders detay sayfası: Ders, kelime kartları ve ders ilerlemesi bilgilerini içerir.
        public async Task<IActionResult> LessonDetail(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Dersin detay bilgilerini al
            LessonViewModel lesson;
            try
            {
                lesson = await _httpClient.GetFromJsonAsync<LessonViewModel>($"api/lessons/{id}");
            }
            catch (System.Text.Json.JsonException)
            {
                // JSON parse hatası varsa NotFound döndürün
                return NotFound("Ders bilgisi okunamadı.");
            }
    
            if (lesson == null)
                return NotFound();

            // Dersin kelime kartlarını getir (endpoint'inizi kontrol edin, örneğin tekil "lesson" olabilir)
            List<WordCardViewModel> wordCards;
            try
            {
                wordCards = await _httpClient.GetFromJsonAsync<List<WordCardViewModel>>($"api/wordcards/lessons/{id}");
            }
            catch (HttpRequestException)
            {
                wordCards = new List<WordCardViewModel>();
            }

            // Kelime kartı ilerlemelerini getir
            List<StudentWordCardProgressViewModel> cardProgresses;
            try
            {
                cardProgresses = await _httpClient.GetFromJsonAsync<List<StudentWordCardProgressViewModel>>($"api/progress/wordcards/{studentId}/lesson/{id}");
            }
            catch (HttpRequestException)
            {
                cardProgresses = new List<StudentWordCardProgressViewModel>();
            }

            // Ders ilerlemesini al veya oluştur
            var lessonProgress = await GetOrCreateLessonProgress(studentId, id);
    
            var viewModel = new LessonDetailViewModel
            {
                Lesson = lesson,
                WordCards = wordCards?.Select(card =>
                {
                    var progress = cardProgresses?.FirstOrDefault(p => p.WordCardId == card.Id);
                    return new WordCardWithProgressViewModel
                    {
                        WordCard = card,
                        Progress = progress ?? new StudentWordCardProgressViewModel()
                    };
                }).ToList() ?? new List<WordCardWithProgressViewModel>(),
                LessonProgress = lessonProgress
            };

            return View(viewModel);
        }

        // Kelime kartı ilerlemesini güncellemek için kullanılan POST metodu.
        [HttpPost]
        public async Task<IActionResult> UpdateWordCardProgress([FromBody] UpdateWordCardProgressViewModel model)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // API'ye ilerleme güncelleme isteği gönder
            var response = await _httpClient.PostAsJsonAsync("api/progress/wordcards/update", new
            {
                StudentId = studentId,
                WordCardId = model.WordCardId,
                IsLearned = model.IsLearned,
                IsMarkedForReview = model.IsMarkedForReview
            });

            if (!response.IsSuccessStatusCode)
                return BadRequest();

            return Ok();
        }

        // Öğrenci için ders ilerlemesini getirir; mevcut değilse yeni oluşturur.
        private async Task<StudentLessonProgressViewModel> GetOrCreateLessonProgress(string studentId, int lessonId)
        {
            StudentLessonProgressViewModel progress = null;
            try
            {
                // API'den ders ilerlemesi çekmeye çalışıyoruz
                progress = await _httpClient.GetFromJsonAsync<StudentLessonProgressViewModel>($"api/progress/lessons/{studentId}/{lessonId}");
            }
            catch (System.Text.Json.JsonException)
            {
                // Yanıt boş veya geçersiz JSON içeriyorsa, progress null olsun
                progress = null;
            }
            catch (HttpRequestException)
            {
                progress = null;
            }
    
            // Eğer progress bulunamadıysa (null ise), create işlemini tetikleyelim
            if (progress == null)
            {
                var response = await _httpClient.PostAsJsonAsync("api/progress/lessons/create", new
                {
                    StudentId = studentId,
                    LessonId = lessonId
                });

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        progress = await response.Content.ReadFromJsonAsync<StudentLessonProgressViewModel>();
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        progress = null;
                    }
                }
            }

            return progress ?? new StudentLessonProgressViewModel();
        }
    }
}
