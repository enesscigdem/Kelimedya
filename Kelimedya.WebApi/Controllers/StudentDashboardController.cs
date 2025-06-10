using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.WebAPI.Models;
using Kelimedya.Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/student/dashboard")]
    [ApiController]
    public class StudentDashboardController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public StudentDashboardController(KelimedyaDbContext context)
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

            var gameStats = await _context.Games
                .Where(g => !g.IsDeleted)
                .GroupJoin(
                    _context.StudentGameStatistics.Where(s => s.StudentId == studentId),
                    game => game.Id,
                    stat => stat.GameId,
                    (game, stats) => new { Game = game, Stats = stats }
                )
                .Select(g => new GameStatSummaryDto
                {
                    GameId = g.Game.Id,
                    GameTitle = g.Game.Title,
                    PlayCount = g.Stats.Count(),
                    AverageScore = g.Stats.Any() ? g.Stats.Average(s => s.Score) : 0
                })
                .ToListAsync();

            // Ensure gameStats is not null
            gameStats ??= new List<GameStatSummaryDto>();
            
            var dashboardDto = new StudentDashboardDto
            {
                TotalCourses = totalCourses,
                CompletedLessons = completedLessons,
                LearnedWords = learnedWords,
                CourseProgresses = courseProgresses,
                LessonProgresses = lessonProgressList,
                GameStats = gameStats
            };

            return Ok(dashboardDto);
        }
    }
}
