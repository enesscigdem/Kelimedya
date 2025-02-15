using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paragraph.Core.Entities;
using Paragraph.Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace Paragraph.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ParagraphDbContext _context;
        public LessonsController(ParagraphDbContext context)
        {
            _context = context;
        }

        // GET: api/lessons
        [HttpGet]
        public async Task<IActionResult> GetLessons()
        {
            var lessons = await _context.Lessons.Where(l => !l.IsDeleted).ToListAsync();
            return Ok(lessons);
        }

        // GET: api/lessons/bycourse/{courseId}
        [HttpGet("bycourse/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourse(int courseId)
        {
            var lessons = await _context.Lessons.Where(l => l.CourseId == courseId && !l.IsDeleted).ToListAsync();
            return Ok(lessons);
        }

        // GET: api/lessons/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null || lesson.IsDeleted)
                return NotFound();
            return Ok(lesson);
        }

        // POST: api/lessons
        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLesson), new { id = lesson.Id }, lesson);
        }

        // PUT: api/lessons/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] Lesson lesson)
        {
            if (id != lesson.Id)
                return BadRequest();
            _context.Entry(lesson).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Lessons.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/lessons/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                return NotFound();
            lesson.IsDeleted = true;
            lesson.IsActive = false;
            _context.Entry(lesson).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Lesson deleted successfully" });
        }
    }
}
