using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace Kelimedya.WebAPI.Controllers;

[Route("api/quizquestions")]
[ApiController]
public class WordCardQuizQuestionsController : ControllerBase
{
    private readonly KelimedyaDbContext _context;
    public WordCardQuizQuestionsController(KelimedyaDbContext context)
    {
        _context = context;
    }

    [HttpGet("lesson/{lessonId}")]
    public async Task<IActionResult> GetQuestionsForLesson(int lessonId)
    {
        var cardIds = await _context.WordCards
            .Where(w => w.LessonId == lessonId && !w.IsDeleted)
            .Select(w => w.Id)
            .ToListAsync();

        var questions = await _context.WordCardQuizQuestions
            .Where(q => cardIds.Contains(q.WordCardId) && !q.IsDeleted)
            .ToListAsync();

        return Ok(questions);
    }
}
