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
            LessonId = dto.LessonId,
            CourseId = dto.CourseId,
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
