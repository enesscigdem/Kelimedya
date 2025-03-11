using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paragraph.Core.Entities;
using Paragraph.Persistence;
using Paragraph.WebAPI.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Paragraph.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ParagraphDbContext _context;
        public CoursesController(ParagraphDbContext context)
        {
            _context = context;
        }

        // GET: api/courses
        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses.Where(c => !c.IsDeleted).ToListAsync();
            return Ok(courses);
        }

        // GET: api/courses/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCourses()
        {
            var courses = await _context.Courses
                .Where(c => c.IsActive && !c.IsDeleted)
                .ToListAsync();
            return Ok(courses);
        }

        // GET: api/courses/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.IsDeleted)
                return NotFound();
            return Ok(course);
        }

        // POST: api/courses
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromForm] CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            if (dto.ImageFile != null)
            {
                course.ImageUrl = await SaveFileAsync(dto.ImageFile, "courses");
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        // PUT: api/courses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromForm] CourseUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.IsDeleted)
                return NotFound();

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.LessonCount = dto.LessonCount;
            course.WordCount = dto.WordCount;
            course.IsActive = dto.IsActive;
            course.ModifiedAt = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {
                // Eski resmi silmek isterseniz ilgili kod ekleyin.
                course.ImageUrl = await SaveFileAsync(dto.ImageFile, "courses");
            }

            _context.Entry(course).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Courses.Any(c => c.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/courses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();
            course.IsDeleted = true;
            course.IsActive = false;
            _context.Entry(course).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Course deleted successfully." });
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

            return $"{Request.Scheme}://{Request.Host}/uploads/{folder}/{fileName}";
        }
    }
}
