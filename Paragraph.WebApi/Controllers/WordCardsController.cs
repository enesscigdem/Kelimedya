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

        // POST: api/wordcards
        [HttpPost]
        public async Task<IActionResult> CreateWordCard([FromBody] WordCard card)
        {
            _context.WordCards.Add(card);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWordCard), new { id = card.Id }, card);
        }

        // PUT: api/wordcards/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWordCard(int id, [FromBody] WordCard card)
        {
            if (id != card.Id)
                return BadRequest();
            _context.Entry(card).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.WordCards.Any(e => e.Id == id))
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
    }
}
