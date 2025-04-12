using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paragraph.Core.Entities;
using Paragraph.WebAPI.Models;
using Paragraph.Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace Paragraph.WebAPI.Controllers
{
    [Route("api/student/dashboard")]
    [ApiController]
    public class StudentDashboardController : ControllerBase
    {
        private readonly ParagraphDbContext _context;
        public StudentDashboardController(ParagraphDbContext context)
        {
            _context = context;
        }

        // GET: api/student/dashboard/{studentId}
        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetDashboardData(string studentId)
        {
            var lessonProgresses = await _context.StudentLessonProgresses
                .Include(p => p.Lesson)
                    .ThenInclude(l => l.Course)
                .Where(p => p.StudentId == studentId)
                .ToListAsync();

            var totalCourses = lessonProgresses
                .Select(p => p.Lesson.CourseId)
                .Distinct()
                .Count();

            var completedLessons = lessonProgresses.Count(p => p.IsCompleted);

            var learnedWords = await _context.StudentWordCardProgresses
                .Where(p => p.StudentId == studentId && p.IsLearned)
                .CountAsync();

            var courseProgresses = lessonProgresses
                .GroupBy(p => p.Lesson.Course)
                .Select(g => new CourseProgressDto
                {
                    CourseId = g.Key.Id,
                    CourseTitle = g.Key.Title,
                    AverageCompletion = g.Average(p => p.CompletionPercentage)
                })
                .ToList();

            var lessonProgressList = lessonProgresses
                .Select(p => new LessonProgressDto
                {
                    LessonId = p.LessonId,
                    LessonTitle = p.Lesson.Title,
                    CompletionPercentage = p.CompletionPercentage,
                    IsCompleted = p.IsCompleted
                })
                .ToList();

            var dashboardDto = new StudentDashboardDto
            {
                TotalCourses = totalCourses,
                CompletedLessons = completedLessons,
                LearnedWords = learnedWords,
                CourseProgresses = courseProgresses,
                LessonProgresses = lessonProgressList
            };

            return Ok(dashboardDto);
        }
    }
}
