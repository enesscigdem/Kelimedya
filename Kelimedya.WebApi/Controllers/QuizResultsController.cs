using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using Kelimedya.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kelimedya.WebAPI.Controllers;

[Route("api/quizresults")]
[ApiController]
public class QuizResultsController : ControllerBase
{
    private readonly KelimedyaDbContext _context;

    public QuizResultsController(KelimedyaDbContext context)
    {
        _context = context;
    }

    [HttpPost("record")]
    public async Task<IActionResult> Record([FromBody] RecordQuizResultDto dto)
    {
        if (string.IsNullOrEmpty(dto.StudentId))
            return BadRequest("StudentId is required");

        var result = new StudentQuizResult
        {
            StudentId = dto.StudentId,
            LessonId = dto.LessonId ?? 0,
            CourseId = dto.CourseId ?? 0,
            TotalQuestions = dto.TotalQuestions,
            CorrectAnswers = dto.CorrectAnswers,
            Score = dto.Score,
            CompletedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        _context.StudentQuizResults.Add(result);

        // Mark lesson progress as completed when quiz is recorded
        var progress = await _context.StudentLessonProgresses
            .FirstOrDefaultAsync(p => p.StudentId == dto.StudentId && p.LessonId == dto.LessonId);

        if (progress != null)
        {
            progress.IsCompleted = true;
            progress.CompletionPercentage = 100;
            progress.LastAccessDate = DateTime.UtcNow;
            progress.ModifiedAt = DateTime.UtcNow;
            _context.StudentLessonProgresses.Update(progress);
        }
        else
        {
            var newProgress = new StudentLessonProgress
            {
                StudentId = dto.StudentId,
                LessonId = dto.LessonId ?? 0,
                LearnedWordCardsCount = 0,
                CompletionPercentage = 100,
                StartDate = DateTime.UtcNow,
                LastAccessDate = DateTime.UtcNow,
                IsCompleted = true,
                TotalAttempts = 0,
                TotalTimeSpentSeconds = 0,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            _context.StudentLessonProgresses.Add(newProgress);
        }

        await _context.SaveChangesAsync();
        return Ok(result);
    }

    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetResults(string studentId)
    {
        var results = await _context.StudentQuizResults
            .Where(r => r.StudentId == studentId)
            .ToListAsync();
        return Ok(results);
    }
}
