using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paragraph.Core.Entities;
using Paragraph.Persistence;
using Paragraph.WebAPI.Models;
using System;
using System.IO;
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
        public async Task<IActionResult> CreateLesson([FromForm] LessonCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lesson = new Lesson
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                Description = dto.Description,
                SequenceNo = dto.SequenceNo,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            if (dto.ImageFile != null)
            {
                lesson.ImageUrl = await SaveFileAsync(dto.ImageFile, "lessons");
            }

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLesson), new { id = lesson.Id }, lesson);
        }

        // PUT: api/lessons/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromForm] LessonUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null || lesson.IsDeleted)
                return NotFound();

            lesson.CourseId = dto.CourseId;
            lesson.Title = dto.Title;
            lesson.Description = dto.Description;
            lesson.SequenceNo = dto.SequenceNo;
            lesson.ModifiedAt = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {
                // (Opsiyonel: eski dosyayı silmek isterseniz ilgili kod eklenebilir)
                lesson.ImageUrl = await SaveFileAsync(dto.ImageFile, "lessons");
            }

            _context.Entry(lesson).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Lessons.Any(l => l.Id == id))
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

        // Yardımcı metot: Dosya kaydetme
        private async Task<string?> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Absolute URL üretelim (API projesi https://localhost:5001 çalışıyor)
            return $"{Request.Scheme}://{Request.Host}/uploads/{folder}/{fileName}";
        }

    }
}
