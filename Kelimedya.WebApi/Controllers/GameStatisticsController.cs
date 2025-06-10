using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using Kelimedya.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kelimedya.WebAPI.Controllers;

[Route("api/gamestats")]
[ApiController]
public class GameStatisticsController : ControllerBase
{
    private readonly KelimedyaDbContext _context;
    public GameStatisticsController(KelimedyaDbContext context)
    {
        _context = context;
    }

    [HttpPost("record")]
    public async Task<IActionResult> RecordStatistic([FromBody] RecordGameStatisticDto dto)
    {
        if (string.IsNullOrEmpty(dto.StudentId))
            return BadRequest("StudentId is required");

        var stat = new StudentGameStatistic
        {
            StudentId = dto.StudentId,
            GameId = dto.GameId,
            Score = dto.Score,
            DurationSeconds = dto.DurationSeconds,
            PlayedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        _context.StudentGameStatistics.Add(stat);
        await _context.SaveChangesAsync();
        return Ok(stat);
    }

    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetStats(string studentId, int? gameId = null)
    {
        var query = _context.StudentGameStatistics
            .Where(s => s.StudentId == studentId);
        if (gameId.HasValue)
            query = query.Where(s => s.GameId == gameId.Value);

        var stats = await query.ToListAsync();
        return Ok(stats);
    }
}
