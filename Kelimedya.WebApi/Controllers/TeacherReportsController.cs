using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Persistence;
using Kelimedya.WebAPI.Models; // DTO'lar için (aşağıda)
using Kelimedya.Core.Interfaces.Business;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Kelimedya.Core.Entities;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/teacher/reports")]
    [ApiController]
    public class TeacherReportsController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public TeacherReportsController(KelimedyaDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        // === Varsa önceki "students" endpoint'inizi aynen koruyun ===
        // GET: api/teacher/reports/students
        [HttpGet("students")]
        public async Task<IActionResult> GetStudentReports()
        {
            var teacherId = _currentUserService.GetUserId();
            var myStudentIds = await _context.Users
                .Where(u => u.TeacherId == teacherId)
                .Select(u => u.Id.ToString())
                .ToListAsync();

            var reports = await _context.StudentLessonProgresses
                .Include(slp => slp.Lesson).ThenInclude(l => l.Course)
                .Where(slp => myStudentIds.Contains(slp.StudentId))
                .GroupBy(slp => slp.StudentId)
                .Select(g => new StudentReportDto
                {
                    StudentId = g.Key,
                    FullName = _context.Users
                        .Where(u => u.Id.ToString() == g.Key)
                        .Select(u => u.Name + " " + u.Surname)
                        .FirstOrDefault() ?? "Bilinmiyor",
                    CompletedLessons = g
                        .Where(slp => slp.IsCompleted)
                        .Select(slp => new LessonReportDto
                        {
                            LessonId = slp.LessonId,
                            Title = slp.Lesson.Title,
                            CourseTitle = slp.Lesson.Course.Title,
                            CompletionPercentage = slp.CompletionPercentage,
                            StartDate = slp.StartDate,
                            LastAccessDate = slp.LastAccessDate
                        }).ToList(),
                    LearnedWords = (from swcp in _context.StudentWordCardProgresses
                        join wc in _context.WordCards on swcp.WordCardId equals wc.Id
                        where swcp.IsLearned && swcp.StudentId == g.Key
                        select new WordReportDto
                        {
                            WordCardId = wc.Id,
                            Word = wc.Word,
                            Synonym = wc.Synonym,
                            Definition = wc.Definition,
                            ExampleSentence = wc.ExampleSentence,
                            ViewCount = swcp.ViewCount
                        }).ToList()
                })
                .ToListAsync();

            return Ok(reports);
        }

        // === Öğretmen KPI özeti ===
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var teacherId = _currentUserService.GetUserId();
            var myStudentIds = await _context.Users
                .Where(u => u.TeacherId == teacherId)
                .Select(u => u.Id.ToString())
                .ToListAsync();

            var lessonQ = _context.StudentLessonProgresses.Where(s => myStudentIds.Contains(s.StudentId));
            var wordQ = _context.StudentWordCardProgresses.Where(w =>
                myStudentIds.Contains(w.StudentId) && w.IsLearned);

            // Bu iki tablo adını kendi entity'lerinize göre uyarlayın:
            var quizQ = _context.Set<StudentQuizResult>().Where(q => myStudentIds.Contains(q.StudentId));
            var gameQ = _context.Set<StudentGameStatistic>().Where(g => myStudentIds.Contains(g.StudentId));

            var dto = new TeacherOverviewDto
            {
                TotalStudents = myStudentIds.Count,
                AvgLessonCompletion = await lessonQ.AnyAsync()
                    ? await lessonQ.AverageAsync(x => (double)x.CompletionPercentage)
                    : 0,
                TotalLearnedWords = await wordQ.CountAsync(),
                TotalQuizAttempts = await quizQ.CountAsync(),
                AvgQuizScore = await quizQ.AnyAsync() ? await quizQ.AverageAsync(x => (double)x.Score) : 0,
                TotalGamePoints = await gameQ.AnyAsync() ? await gameQ.SumAsync(x => x.Score) : 0
            };
            return Ok(dto);
        }

        [HttpGet("transcript/{studentId}")]
        public async Task<IActionResult> GetTranscript(string studentId)
        {
            var teacherId = _currentUserService.GetUserId();
            var isMine = await _context.Users.AnyAsync(u => u.Id.ToString() == studentId && u.TeacherId == teacherId);
            if (!isMine) return Forbid();

            var studentName = await _context.Users
                .Where(u => u.Id.ToString() == studentId)
                .Select(u => u.Name + " " + u.Surname)
                .FirstOrDefaultAsync() ?? ("Öğrenci " + studentId);

            var lessons = await _context.StudentLessonProgresses
                .Include(s => s.Lesson).ThenInclude(l => l.Course)
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

            var learnedWordCount = await _context.StudentWordCardProgresses
                .CountAsync(w => w.StudentId == studentId && w.IsLearned);

            // ⬇️ Doğru entity’ler
            var quizQ = _context.Set<StudentQuizResult>()
                .Where(q => q.StudentId == studentId /* && !q.IsDeleted && q.IsActive */);
            var gameQ = _context.Set<StudentGameStatistic>()
                .Where(g => g.StudentId == studentId /* && !g.IsDeleted && g.IsActive */);

            var totalQuizAttempts = await quizQ.CountAsync();
            var avgQuizScore = await quizQ.AnyAsync() ? await quizQ.AverageAsync(x => (double)x.Score) : 0;
            var totalGamePoints = await gameQ.AnyAsync() ? await gameQ.SumAsync(x => x.Score) : 0;

            // ⬇️ Quiz’lerde tarih alanı CompletedAt
            var lastQuiz = await quizQ
                .OrderByDescending(x => x.CompletedAt)
                .Take(5)
                .Select(x => new QuizAttemptItemDto
                {
                    Date = x.CompletedAt,
                    TotalQuestions = x.TotalQuestions,
                    CorrectAnswers = x.CorrectAnswers,
                    Score = x.Score
                }).ToListAsync();

            var html = BuildTranscriptHtml(studentName, lessons, learnedWordCount, totalQuizAttempts, avgQuizScore,
                totalGamePoints, lastQuiz);

            return Ok(new TranscriptDto
            {
                StudentName = studentName,
                PdfDownloadUrl = Url.Action(nameof(DownloadTranscript), new { studentId }),
                HtmlContent = html,
                TotalQuizAttempts = totalQuizAttempts,
                AvgQuizScore = avgQuizScore,
                TotalGamePoints = totalGamePoints,
                LastQuizAttempts = lastQuiz
            });
        }


      [HttpGet("detailed/{studentId}")]
public async Task<IActionResult> GetDetailedReport(string studentId)
{
    var teacherId = _currentUserService.GetUserId();
    var isMine = await _context.Users.AnyAsync(u => u.Id.ToString() == studentId && u.TeacherId == teacherId);
    if (!isMine) return Forbid();

    var studentName = await _context.Users
        .Where(u => u.Id.ToString() == studentId)
        .Select(u => u.Name + " " + u.Surname)
        .FirstOrDefaultAsync() ?? ("Öğrenci " + studentId);

    var lessons = await _context.StudentLessonProgresses
        .Include(s => s.Lesson).ThenInclude(l => l.Course)
        .Where(s => s.StudentId == studentId)
        .ToListAsync();

    var learnedWordCount = await _context.StudentWordCardProgresses
        .CountAsync(w => w.StudentId == studentId && w.IsLearned);
    var avgCompletion = lessons.Any() ? lessons.Average(x => (double)x.CompletionPercentage) : 0;

    var quizQ = _context.Set<StudentQuizResult>()
        .Where(q => q.StudentId == studentId /* && q.IsActive && !q.IsDeleted */);
    var gameQ = _context.Set<StudentGameStatistic>()
        .Where(g => g.StudentId == studentId /* && g.IsActive && !g.IsDeleted */);

    var totalQuizAttempts = await quizQ.CountAsync();
    var avgQuizScore      = await quizQ.AnyAsync() ? await quizQ.AverageAsync(x => (double)x.Score) : 0;
    var totalGamePoints   = await gameQ.AnyAsync() ? await gameQ.SumAsync(x => x.Score) : 0;

    var lastQuiz = await quizQ.OrderByDescending(x => x.CompletedAt).Take(10)
        .Select(x => new QuizAttemptItemDto {
            Date = x.CompletedAt,
            TotalQuestions = x.TotalQuestions,
            CorrectAnswers = x.CorrectAnswers,
            Score = x.Score
        }).ToListAsync();

    // Game title join
    var lastGames = await (
        from gs in gameQ
        join gm in _context.Games on gs.GameId equals gm.Id into gjoin
        from gm in gjoin.DefaultIfEmpty()
        orderby gs.PlayedAt descending
        select new GamePointItemDto {
            Date = gs.PlayedAt,
            GameName = gm != null ? gm.Title : ("Oyun #" + gs.GameId),
            Score = gs.Score
        }
    ).Take(10).ToListAsync();

    // Kelime istatistikleri (mevcut şemaya göre)
    var topWords = await (
        from sw in _context.StudentWordCardProgresses
        join wc in _context.WordCards on sw.WordCardId equals wc.Id
        where sw.StudentId == studentId
        orderby sw.ViewCount descending, sw.ResponseTimeTotalSeconds descending
        select new WordStatsDto {
            WordCardId = wc.Id,
            Word = wc.Word,
            Definition = wc.Definition,
            ViewCount = sw.ViewCount,
            TimeSpentSeconds = sw.ResponseTimeTotalSeconds,
            AvgResponseTimeSeconds = sw.AverageResponseTimeSeconds,
            LastSeenDate = sw.LastSeenDate
        }
    ).Take(50).ToListAsync();

    // Soru-bazı cevap verisi yok -> boş liste
    var wrongAnswers = new List<WrongAnswerDto>(); 

    return Ok(new DetailedReportDto {
        StudentName = studentName,
        PdfDownloadUrl = Url.Action(nameof(DownloadDetailedReport), new { studentId }),
        CompletedLessonCount = lessons.Count(x => x.IsCompleted),
        LearnedWordCount = learnedWordCount,
        AvgLessonCompletion = avgCompletion,
        TotalQuizAttempts = totalQuizAttempts,
        AvgQuizScore = avgQuizScore,
        TotalGamePoints = totalGamePoints,
        LastQuizAttempts = lastQuiz,
        LastGamePoints = lastGames,
        TopWords = topWords,
        WrongAnswers = wrongAnswers
    });
}



        // === PDF indirme uçları (API üzerinden) ===
        [HttpGet("download/transcript/{studentId}")]
        public IActionResult DownloadTranscript(string studentId)
        {
            // Demo amaçlı statik PDF. Gerçekte dosyayı üretebilir/generate edebilirsiniz.
            var bytes = System.IO.File.ReadAllBytes("wwwroot/sample_transcript.pdf");
            return File(bytes, "application/pdf", "transcript.pdf");
        }

        [HttpGet("download/detailed/{studentId}")]
        public IActionResult DownloadDetailedReport(string studentId)
        {
            var bytes = System.IO.File.ReadAllBytes("wwwroot/sample_detailed_report.pdf");
            return File(bytes, "application/pdf", "detailed_report.pdf");
        }

        // --- Şık karne HTML şablonu ---
        private string BuildTranscriptHtml(
            string studentName,
            List<StudentLessonProgress> lessons,
            int learnedWordCount,
            int totalQuizAttempts,
            double avgQuizScore,
            int totalGamePoints,
            List<QuizAttemptItemDto> lastQuiz)
        {
            var sb = new StringBuilder();
            sb.Append(@"
<div class='km-transcript'>
  <div class='km-head'>
    <div class='km-brand'>
      <img src='/theme/web_UI/assets/img/home_5/kelimedya-logo.png' alt='Logo' />
      <div>
        <h2>Öğrenci Karnesi</h2>
        <p class='km-student'>" + System.Net.WebUtility.HtmlEncode(studentName) + @"</p>
      </div>
    </div>
    <div class='km-badges'>
      <div class='km-badge'><span>Öğrenilen Kelime</span><strong>" + learnedWordCount + @"</strong></div>
      <div class='km-badge'><span>Quiz Denemesi</span><strong>" + totalQuizAttempts + @"</strong></div>
      <div class='km-badge'><span>Ort. Quiz Skoru</span><strong>" + avgQuizScore.ToString("0") + @"</strong></div>
      <div class='km-badge'><span>Toplam Puan</span><strong>" + totalGamePoints + @"</strong></div>
    </div>
  </div>
  <div class='km-body'>
    <h3>Ders İlerlemesi</h3>
    <div class='km-courses'>");

            foreach (var g in lessons.GroupBy(l => l.Lesson.Course.Title))
            {
                sb.Append("<div class='km-course-group'><h4>" + System.Net.WebUtility.HtmlEncode(g.Key) + "</h4>");
                foreach (var p in g)
                {
                    var pct = (int)p.CompletionPercentage;
                    sb.Append(@"
            <div class='km-course-item'>
              <div class='km-ci-title'>" + System.Net.WebUtility.HtmlEncode(p.Lesson.Title) + @"</div>
              <div class='km-ci-bar'><div style='width:" + pct + @"%'></div></div>
              <div class='km-ci-pct'>" + pct + @"%</div>
            </div>");
                }

                sb.Append("</div>");
            }

            sb.Append(@"</div>
    <h3>Son Quiz Sonuçları</h3>
    <table class='km-table'>
      <thead><tr><th>Tarih</th><th>Soru</th><th>Doğru</th><th>Puan</th></tr></thead>
      <tbody>");
            foreach (var q in lastQuiz)
            {
                sb.Append("<tr><td>" + q.Date.ToString("dd.MM.yyyy HH:mm") + "</td><td>" + q.TotalQuestions +
                          "</td><td>" + q.CorrectAnswers + "</td><td>" + q.Score + "</td></tr>");
            }

            sb.Append(@"</tbody></table>
  </div>
  <div class='km-foot'>Bu belge öğrencinin döneme ait performans özetidir.</div>
</div>

<style>
.km-transcript{background:#fff;border-radius:20px;overflow:hidden;border:1px solid #fde68a}
.km-head{display:flex;justify-content:space-between;gap:16px;padding:20px;background:linear-gradient(90deg,#f97316,#ea580c);color:#fff}
.km-brand{display:flex;gap:12px;align-items:center}
.km-brand img{height:42px}
.km-brand h2{margin:0;font-size:22px}
.km-student{margin:2px 0 0;opacity:.9}
.km-badges{display:flex;gap:12px;flex-wrap:wrap;align-items:center}
.km-badge{background:rgba(255,255,255,.12);border:1px solid rgba(255,255,255,.25);border-radius:12px;padding:8px 12px}
.km-badge span{display:block;font-size:12px;opacity:.85}
.km-badge strong{display:block;font-size:18px;margin-top:3px}
.km-body{padding:20px}
.km-courses{display:grid;gap:14px}
.km-course-group h4{margin:4px 0 10px;color:#9a3412}
.km-course-item{display:grid;grid-template-columns:1fr auto 56px;align-items:center;gap:12px;padding:10px;border:1px solid #fee2e2;border-radius:12px;background:linear-gradient(180deg,#fff7ed,#fffbeb)}
.km-ci-title{font-weight:700;color:#7c2d12}
.km-ci-bar{height:10px;background:#ffe6cc;border-radius:999px;overflow:hidden;border:1px solid #fdba74}
.km-ci-bar>div{height:100%;background:linear-gradient(90deg,#fb923c,#f97316)}
.km-ci-pct{font-weight:800;color:#9a3412;text-align:right}
.km-table{width:100%;border-collapse:collapse;margin-top:8px}
.km-table th,.km-table td{border:1px solid #fde68a;padding:8px}
.km-table thead th{background:#fff7ed;color:#7c2d12}
.km-foot{padding:12px 20px;border-top:1px solid #fde68a;background:#fffbea;color:#7c2d12;font-size:12px}
</style>
");
            return sb.ToString();
        }
    }
}