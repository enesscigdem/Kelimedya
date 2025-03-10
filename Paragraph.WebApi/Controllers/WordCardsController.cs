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
    public class WordCardsController : ControllerBase
    {
        private readonly ParagraphDbContext _context;
        public WordCardsController(ParagraphDbContext context)
        {
            _context = context;
        }

        // GET: api/wordcards
        [HttpGet]
        public async Task<IActionResult> GetWordCards()
        {
            var cards = await _context.WordCards.Where(w => !w.IsDeleted).ToListAsync();
            return Ok(cards);
        }

        // GET: api/wordcards/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWordCard(int id)
        {
            var card = await _context.WordCards.FindAsync(id);
            if (card == null || card.IsDeleted)
                return NotFound();
            return Ok(card);
        }

        // GET : api/wordcards/lessons/{lessonId}
        [HttpGet("lessons/{lessonId}")]
        public async Task<IActionResult> GetWordCardsByLesson(int lessonId)
        {
            var cards = await _context.WordCards.Where(w => w.LessonId == lessonId && !w.IsDeleted).ToListAsync();
            return Ok(cards);
        }
        
        // POST: api/wordcards
        [HttpPost]
        public async Task<IActionResult> CreateWordCard([FromForm] WordCardCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var card = new WordCard
            {
                LessonId = dto.LessonId,
                Word = dto.Word,
                Definition = dto.Definition,
                ExampleSentence = dto.ExampleSentence,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            if (dto.ImageFile != null)
            {
                card.ImageUrl = await SaveFileAsync(dto.ImageFile, "wordcards");
            }

            _context.WordCards.Add(card);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWordCard), new { id = card.Id }, card);
        }

        // PUT: api/wordcards/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWordCard(int id, [FromForm] WordCardUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var card = await _context.WordCards.FindAsync(id);
            if (card == null || card.IsDeleted)
                return NotFound();

            card.LessonId = dto.LessonId;
            card.Word = dto.Word;
            card.Definition = dto.Definition;
            card.ExampleSentence = dto.ExampleSentence;
            card.ModifiedAt = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {
                card.ImageUrl = await SaveFileAsync(dto.ImageFile, "wordcards");
            }

            _context.Entry(card).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.WordCards.Any(w => w.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/wordcards/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWordCard(int id)
        {
            var card = await _context.WordCards.FindAsync(id);
            if (card == null)
                return NotFound();
            card.IsDeleted = true;
            card.IsActive = false;
            _context.Entry(card).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "WordCard deleted successfully" });
        }

        // Yardımcı metot: Dosya kaydetme (aynı mantık)
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
