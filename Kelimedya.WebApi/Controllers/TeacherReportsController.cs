using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using Kelimedya.WebAPI.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kelimedya.Core.Interfaces.Business;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/teacher/reports")]
    [ApiController]
    public class TeacherReportsController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public TeacherReportsController(
            KelimedyaDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

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
                        .Select(slp => new LessonReportDto {
                            LessonId = slp.LessonId,
                            Title = slp.Lesson.Title,
                            CourseTitle = slp.Lesson.Course.Title,
                            CompletionPercentage = slp.CompletionPercentage,
                            StartDate = slp.StartDate,
                            LastAccessDate = slp.LastAccessDate
                        }).ToList(),
                    LearnedWords = (from swcp in _context.StudentWordCardProgresses
                                    join wc in _context.WordCards
                                      on swcp.WordCardId equals wc.Id
                                    where swcp.IsLearned && swcp.StudentId == g.Key
                                    select new WordReportDto {
                                       WordCardId = wc.Id,
                                       Word = wc.Word,
                                       Definition = wc.Definition,
                                       ExampleSentence = wc.ExampleSentence,
                                       ViewCount = swcp.ViewCount
                                    }).ToList()
                })
                .ToListAsync();

            return Ok(reports);
        }

        // GET: api/teacher/reports/transcript/{studentId}
        [HttpGet("transcript/{studentId}")]
        public async Task<IActionResult> GetTranscript(string studentId)
        {
            var teacherId = _currentUserService.GetUserId();
            var isMine = await _context.Users
                .AnyAsync(u => u.Id.ToString() == studentId && u.TeacherId == teacherId);

            if (!isMine)
                return Forbid();
            
            var transcriptData = await _context.StudentLessonProgresses
                .Include(slp => slp.Lesson).ThenInclude(l => l.Course)
                .Where(slp => slp.StudentId == studentId)
                .ToListAsync();

            if (transcriptData == null || !transcriptData.Any())
            {
                return Ok(new TranscriptDto
                {
                    HtmlContent = "<p>Öğrenci için karnede veri bulunamadı.</p>",
                    PdfDownloadUrl = Url.Action("DownloadTranscript", new { studentId })
                });
            }

            string studentName = "Öğrenci " + studentId;

            var courses = transcriptData.GroupBy(slp => slp.Lesson.Course).ToList();
            var htmlBuilder = new StringBuilder();

            htmlBuilder.Append("<div class='transcript-container'>");
            htmlBuilder.Append("<div class='transcript-header'>");
            htmlBuilder.Append("<img src='/theme/tabler/static/logo.svg' alt='Okul Logo' class='transcript-logo' />");
            htmlBuilder.Append("<h2 class='transcript-title'>Öğrenci Karnesi</h2>");
            htmlBuilder.Append($"<p class='transcript-student'>Öğrenci: {studentName}</p>");
            htmlBuilder.Append("</div>"); // transcript-header

            htmlBuilder.Append("<div class='transcript-body'>");
            htmlBuilder.Append("<div class='transcript-section'>");
            htmlBuilder.Append("<h3 class='section-title'>Ders İlerlemesi</h3>");
            foreach (var group in courses)
            {
                htmlBuilder.Append($"<h4 class='course-group-title'>{group.Key.Title}</h4>");
                htmlBuilder.Append("<div class='course-list'>");
                foreach (var progress in group)
                {
                    htmlBuilder.Append("<div class='course-item'>");
                    htmlBuilder.Append($"<div class='course-title'>{progress.Lesson.Title}</div>");
                    htmlBuilder.Append("<div class='course-progress'>");
                    htmlBuilder.Append($"<div class='progress-bar' style='width:{progress.CompletionPercentage}%;'></div>");
                    htmlBuilder.Append("</div>");
                    htmlBuilder.Append($"<div class='progress-percentage'>{progress.CompletionPercentage:0}%</div>");
                    htmlBuilder.Append("</div>");
                }
                htmlBuilder.Append("</div>"); // course-list
            }
            htmlBuilder.Append("</div>"); // transcript-section

            htmlBuilder.Append("<div class='transcript-section transcript-summary'>");
            htmlBuilder.Append("<h3 class='section-title'>Genel Bilgiler</h3>");
            htmlBuilder.Append($"<p><strong>Tamamlanan Ders:</strong> {transcriptData.Count(s => s.IsCompleted)}</p>");
            htmlBuilder.Append("<p><strong>Öğrenilen Kelime:</strong> 120</p>"); // Replace with real data if available.
            htmlBuilder.Append("</div>"); // transcript-summary

            htmlBuilder.Append("</div>"); // transcript-body
            htmlBuilder.Append("<div class='transcript-footer'>");
            htmlBuilder.Append("<p>Bu karnede yer alan bilgiler, öğrencinin eğitim performansını yansıtmaktadır.</p>");
            htmlBuilder.Append("</div>");
            htmlBuilder.Append("</div>"); // transcript-container

            return Ok(new TranscriptDto
            {
                HtmlContent = htmlBuilder.ToString(),
                PdfDownloadUrl = Url.Action("DownloadTranscript", new { studentId })
            });
        }

        // GET: api/teacher/reports/detailed/{studentId}
        [HttpGet("detailed/{studentId}")]
        public async Task<IActionResult> GetDetailedReport(string studentId)
        {
            var teacherId = _currentUserService.GetUserId();
            var isMine = await _context.Users
                .AnyAsync(u => u.Id.ToString() == studentId && u.TeacherId == teacherId);
            if (!isMine)
                return Forbid();

            var lessonProgresses = await _context.StudentLessonProgresses
                .Include(slp => slp.Lesson).ThenInclude(l => l.Course)
                .Where(slp => slp.StudentId == studentId)
                .ToListAsync();

            var wordProgresses = await _context.StudentWordCardProgresses
                .Include(w => w.WordCard)
                .Where(w => w.StudentId == studentId && w.IsLearned)
                .ToListAsync();


            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<div class='detailed-report-container'>");
            htmlBuilder.Append("<h2>Detaylı Öğrenci Raporu</h2>");
            htmlBuilder.Append("<div class='detailed-section'>");
            htmlBuilder.Append("<h3>Ders İlerlemesi</h3>");
            htmlBuilder.Append("<ul class='lesson-progress-list'>");
            foreach (var progress in lessonProgresses)
            {
                htmlBuilder.Append("<li class='lesson-progress-item'>");
                htmlBuilder.Append($"<span class='lesson-title'>{progress.Lesson.Title} ({progress.Lesson.Course.Title})</span>");
                htmlBuilder.Append($"<span class='progress-value'>{progress.CompletionPercentage:0}%</span>");
                htmlBuilder.Append("</li>");
            }
            htmlBuilder.Append("</ul>");
            htmlBuilder.Append("</div>"); // detailed-section

            htmlBuilder.Append("<div class='detailed-section'>");
            htmlBuilder.Append("<h3>Öğrenilen Kelimeler</h3>");
            htmlBuilder.Append("<ul class='word-progress-list'>");
            foreach (var wp in wordProgresses)
            {
                htmlBuilder.Append("<li class='word-progress-item'>");
                htmlBuilder.Append($"<span class='word'>{wp.WordCard.Word}</span> - ");
                htmlBuilder.Append($"<span class='definition'>{wp.WordCard.Definition}</span> ");
                htmlBuilder.Append($"<span class='viewcount'>(Gösterim: {wp.ViewCount})</span>");
                htmlBuilder.Append("</li>");
            }
            htmlBuilder.Append("</ul>");
            htmlBuilder.Append("</div>"); // detailed-section

            htmlBuilder.Append("</div>"); // detailed-report-container

            return Ok(new DetailedReportDto
            {
                HtmlContent = htmlBuilder.ToString(),
                PdfDownloadUrl = Url.Action("DownloadDetailedReport", new { studentId })
            });
        }

        // GET: api/teacher/reports/download/transcript/{studentId}
        [HttpGet("download/transcript/{studentId}")]
        public IActionResult DownloadTranscript(string studentId)
        {
            // Return a static PDF for demo purposes.
            byte[] pdfBytes = System.IO.File.ReadAllBytes("wwwroot/sample_transcript.pdf");
            return File(pdfBytes, "application/pdf", "transcript.pdf");
        }

        // GET: api/teacher/reports/download/detailed/{studentId}
        [HttpGet("download/detailed/{studentId}")]
        public IActionResult DownloadDetailedReport(string studentId)
        {
            byte[] pdfBytes = System.IO.File.ReadAllBytes("wwwroot/sample_detailed_report.pdf");
            return File(pdfBytes, "application/pdf", "detailed_report.pdf");
        }
    }
}
