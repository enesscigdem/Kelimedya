using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using Kelimedya.WebAPI.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordCardsController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public WordCardsController(KelimedyaDbContext context)
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

        // GET: api/wordcards/{id}/questions
        [HttpGet("{id}/questions")]
        public async Task<IActionResult> GetWordCardQuestions(int id)
        {
            var cardExists = await _context.WordCards.AnyAsync(w => w.Id == id && !w.IsDeleted);
            if (!cardExists)
                return NotFound();

            var questions = await _context.WordCardGameQuestions
                .Where(q => q.WordCardId == id && !q.IsDeleted)
                .ToListAsync();
            return Ok(questions);
        }

        // GET: api/wordcards/{id}/testquestions
        [HttpGet("{id}/testquestions")]
        public async Task<IActionResult> GetWordCardTestQuestions(int id)
        {
            var cardExists = await _context.WordCards.AnyAsync(w => w.Id == id && !w.IsDeleted);
            if (!cardExists)
                return NotFound();

            var questions = await _context.WordCardTestQuestions
                .Where(q => q.WordCardId == id && !q.IsDeleted)
                .ToListAsync();
            return Ok(questions);
        }

        // GET : api/wordcards/lessons/{lessonId}
        [HttpGet("lessons/{lessonId}")]
        public async Task<IActionResult> GetWordCardsByLesson(int lessonId)
        {
            var cards = await _context.WordCards.Where(w => w.LessonId == lessonId && !w.IsDeleted).ToListAsync();
            return Ok(cards);
        }

        // GET: api/wordcards/lessons/{lessonId}/testquestions
        [HttpGet("lessons/{lessonId}/testquestions")]
        public async Task<IActionResult> GetLessonTestQuestions(int lessonId)
        {
            var cardIds = await _context.WordCards
                .Where(w => w.LessonId == lessonId && !w.IsDeleted)
                .Select(w => w.Id)
                .ToListAsync();

            var questions = await _context.WordCardTestQuestions
                .Where(q => cardIds.Contains(q.WordCardId) && !q.IsDeleted)
                .ToListAsync();

            return Ok(questions);
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
                Synonym = dto.Synonym,
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

            if (dto.GameQuestions != null && dto.GameQuestions.Any())
            {
                foreach (var q in dto.GameQuestions)
                {
                    _context.WordCardGameQuestions.Add(new WordCardGameQuestion
                    {
                        WordCardId = card.Id,
                        GameId = q.GameId,
                        QuestionText = q.QuestionText,
                        AnswerText = q.AnswerText,
                        ImageUrl = q.ImageUrl,
                        ImageUrl2 = q.ImageUrl2,
                        ImageUrl3 = q.ImageUrl3,
                        ImageUrl4 = q.ImageUrl4,
                        QuestionType = q.QuestionType,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectOption = q.CorrectOption,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                var gameIds = await _context.Games.Where(g => !g.IsDeleted).Select(g => g.Id).ToListAsync();
                foreach (var gid in gameIds)
                {
                    _context.WordCardGameQuestions.Add(new WordCardGameQuestion
                    {
                        WordCardId = card.Id,
                        GameId = gid,
                        QuestionText = string.Empty,
                        AnswerText = card.Word,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    });
                }
            }

            if (dto.TestQuestions != null && dto.TestQuestions.Any())
            {
                foreach (var t in dto.TestQuestions)
                {
                    _context.WordCardTestQuestions.Add(new WordCardTestQuestion
                    {
                        WordCardId = card.Id,
                        QuestionText = t.QuestionText,
                        OptionA = t.OptionA,
                        OptionB = t.OptionB,
                        OptionC = t.OptionC,
                        OptionD = t.OptionD,
                        CorrectOption = t.CorrectOption,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    });
                }
            }

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
            card.Synonym = dto.Synonym;
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

                var existing = _context.WordCardGameQuestions.Where(q => q.WordCardId == id);
                _context.WordCardGameQuestions.RemoveRange(existing);
                var existingTests = _context.WordCardTestQuestions.Where(t => t.WordCardId == id);
                _context.WordCardTestQuestions.RemoveRange(existingTests);

                if (dto.GameQuestions != null && dto.GameQuestions.Any())
                {
                    foreach (var q in dto.GameQuestions)
                    {
                        _context.WordCardGameQuestions.Add(new WordCardGameQuestion
                        {
                            WordCardId = id,
                            GameId = q.GameId,
                            QuestionText = q.QuestionText,
                            AnswerText = q.AnswerText,
                            ImageUrl = q.ImageUrl,
                            ImageUrl2 = q.ImageUrl2,
                            ImageUrl3 = q.ImageUrl3,
                            ImageUrl4 = q.ImageUrl4,
                            QuestionType = q.QuestionType,
                            OptionA = q.OptionA,
                            OptionB = q.OptionB,
                            OptionC = q.OptionC,
                            OptionD = q.OptionD,
                            CorrectOption = q.CorrectOption,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    var gameIds = await _context.Games.Where(g => !g.IsDeleted).Select(g => g.Id).ToListAsync();
                    foreach (var gid in gameIds)
                    {
                        _context.WordCardGameQuestions.Add(new WordCardGameQuestion
                        {
                            WordCardId = id,
                            GameId = gid,
                            QuestionText = string.Empty,
                            AnswerText = card.Word,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        });
                    }
                }

                if (dto.TestQuestions != null && dto.TestQuestions.Any())
                {
                    foreach (var t in dto.TestQuestions)
                    {
                        _context.WordCardTestQuestions.Add(new WordCardTestQuestion
                        {
                            WordCardId = id,
                            QuestionText = t.QuestionText,
                            OptionA = t.OptionA,
                            OptionB = t.OptionB,
                            OptionC = t.OptionC,
                            OptionD = t.OptionD,
                            CorrectOption = t.CorrectOption,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        });
                    }
                }

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
