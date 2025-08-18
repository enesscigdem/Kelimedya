using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using Kelimedya.WebAPI.Models;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/progress")]
    [ApiController]
    public class ProgressController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public ProgressController(KelimedyaDbContext context)
        {
            _context = context;
        }

        // GET: api/progress/courses/{studentId}
        [HttpGet("courses/{studentId}")]
        public async Task<IActionResult> GetCourseProgress(string studentId)
        {
            var progresses = await _context.StudentLessonProgresses
                .Where(p => p.StudentId == studentId)
                .ToListAsync();
            return Ok(progresses);
        }

        // GET: api/progress/lessons/{studentId}/course/{courseId}
        [HttpGet("lessons/{studentId}/course/{courseId}")]
        public async Task<IActionResult> GetLessonsProgressForCourse(string studentId, int courseId)
        {
            var progresses = await _context.StudentLessonProgresses
                .Where(p => p.StudentId == studentId &&
                            _context.Lessons.Any(l => l.Id == p.LessonId && l.CourseId == courseId))
                .ToListAsync();
            return Ok(progresses);
        }

        // GET: api/progress/lessons/{studentId}/{lessonId}
        [HttpGet("lessons/{studentId}/{lessonId}")]
        public async Task<IActionResult> GetLessonProgress(string studentId, int lessonId)
        {
            var progress = await _context.StudentLessonProgresses
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId);
            return Ok(progress);
        }

        // POST: api/progress/lessons/create
        [HttpPost("lessons/create")]
        public async Task<IActionResult> CreateLessonProgress([FromBody] CreateLessonProgressDto dto)
        {
            if (string.IsNullOrEmpty(dto.StudentId))
                return BadRequest("StudentId is required.");
            
            var progress = new StudentLessonProgress
            {
                StudentId = dto.StudentId,
                LessonId = dto.LessonId,
                StartDate = DateTime.UtcNow,
                LearnedWordCardsCount = 0,
                CompletionPercentage = 0,
                IsCompleted = false,
                TotalAttempts = 0,
                TotalTimeSpentSeconds = 0,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            _context.StudentLessonProgresses.Add(progress);
            await _context.SaveChangesAsync();
            return Ok(progress);
        }

        // POST: api/progress/lessons/update
        [HttpPost("lessons/update")]
        public async Task<IActionResult> UpdateLessonProgress([FromBody] UpdateLessonProgressDto dto)
        {
            if (string.IsNullOrEmpty(dto.StudentId))
                return BadRequest("StudentId is required.");
            
            var progress = await _context.StudentLessonProgresses
                .FirstOrDefaultAsync(p => p.StudentId == dto.StudentId && p.LessonId == dto.LessonId);
            if (progress == null)
            {
                progress = new StudentLessonProgress
                {
                    StudentId = dto.StudentId,
                    LessonId = dto.LessonId,
                    LearnedWordCardsCount = dto.LearnedWordCardsCount,
                    CompletionPercentage = dto.CompletionPercentage,
                    TotalAttempts = dto.TotalAttempts,
                    TotalTimeSpentSeconds = dto.TotalTimeSpentSeconds,
                    StartDate = DateTime.UtcNow,
                    LastAccessDate = DateTime.UtcNow,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                _context.StudentLessonProgresses.Add(progress);
            }
            else
            {
                progress.LearnedWordCardsCount = dto.LearnedWordCardsCount;
                progress.CompletionPercentage = dto.CompletionPercentage;
                progress.TotalAttempts = dto.TotalAttempts;
                progress.TotalTimeSpentSeconds = dto.TotalTimeSpentSeconds;
                progress.LastAccessDate = DateTime.UtcNow;
                progress.ModifiedAt = DateTime.UtcNow;
                _context.StudentLessonProgresses.Update(progress);
            }
            await _context.SaveChangesAsync();
            return Ok(progress);
        }

        // GET: api/progress/wordcards/{studentId}/lesson/{lessonId}
        [HttpGet("wordcards/{studentId}/lesson/{lessonId}")]
        public async Task<IActionResult> GetWordCardProgress(string studentId, int lessonId)
        {
            var progresses = await _context.StudentWordCardProgresses
                .Where(p => p.StudentId == studentId && p.LessonId == lessonId)
                .ToListAsync();
            return Ok(progresses);
        }

        // POST: api/progress/wordcards/update
        [HttpPost("wordcards/update")]
        public async Task<IActionResult> UpdateWordCardProgress([FromBody] UpdateWordCardProgressDto dto)
        {
            if (string.IsNullOrEmpty(dto.StudentId))
                return BadRequest("StudentId is required.");
            
            var progress = await _context.StudentWordCardProgresses
                .FirstOrDefaultAsync(p => p.StudentId == dto.StudentId 
                                          && p.WordCardId == dto.WordCardId 
                                          && p.LessonId == dto.LessonId);
            if (progress == null)
            {
                progress = new StudentWordCardProgress
                {
                    StudentId = dto.StudentId,
                    WordCardId = dto.WordCardId,
                    LessonId = dto.LessonId,
                    IsLearned = dto.IsLearned,
                    ViewCount = 1,
                    ResponseTimeTotalSeconds = dto.ResponseTimeSeconds,
                    FirstSeenDate = DateTime.UtcNow,
                    LastSeenDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                _context.StudentWordCardProgresses.Add(progress);
            }
            else
            {
                progress.IsLearned = dto.IsLearned;
                progress.ViewCount++;
                progress.ResponseTimeTotalSeconds += dto.ResponseTimeSeconds;
                progress.LastSeenDate = DateTime.UtcNow;
                progress.ModifiedAt = DateTime.UtcNow;
                _context.StudentWordCardProgresses.Update(progress);
            }
            await _context.SaveChangesAsync();
            return Ok(progress);
        }

        // Yeni Endpoint: Öğrencinin öğrendiği kelime kartlarını döndürür
        // GET: api/progress/wordcards/learned/{studentId}
        [HttpGet("wordcards/learned/{studentId}")]
        public async Task<IActionResult> GetLearnedWordCards(string studentId, [FromQuery] int? lessonId)
        {
            var query = _context.StudentWordCardProgresses
                .Where(p => p.StudentId == studentId && p.IsLearned);

            if (lessonId.HasValue)
            {
                query = query.Where(p => p.LessonId == lessonId.Value);
            }

            var progresses = await query.ToListAsync();

            var cardIds = progresses.Select(p => p.WordCardId).ToList();
            var cards = await _context.WordCards.Where(w => cardIds.Contains(w.Id)).ToListAsync();
            var questions = await _context.WordCardGameQuestions
                .Where(q => cardIds.Contains(q.WordCardId)).ToListAsync();

            var learnedWords = from progress in progresses
                               join card in cards on progress.WordCardId equals card.Id
                               select new
                               {
                                   Progress = progress,
                                   WordCard = card,
                                   GameQuestions = questions.Where(q => q.WordCardId == card.Id).ToList()
                               };

            return Ok(learnedWords);
        }
    }
}
