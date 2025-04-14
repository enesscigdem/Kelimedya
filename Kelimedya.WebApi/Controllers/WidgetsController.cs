using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WidgetsController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public WidgetsController(KelimedyaDbContext context)
        {
            _context = context;
        }

        // GET: api/widgets
        [HttpGet]
        public async Task<IActionResult> GetWidgets()
        {
            var widgets = await _context.Widgets
                .Where(w => w.IsActive && !w.IsDeleted)
                .ToListAsync();
            return Ok(widgets);
        }

        // GET: api/widgets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWidget(int id)
        {
            var widget = await _context.Widgets.FindAsync(id);
            if (widget == null || widget.IsDeleted)
                return NotFound();
            return Ok(widget);
        }

        // GET: api/widgets/key/{key}
        [HttpGet("key/{key}")]
        public async Task<IActionResult> GetWidgetByKey(string key)
        {
            var widget = await _context.Widgets
                .FirstOrDefaultAsync(w => w.Key == key && w.IsActive && !w.IsDeleted);
            if (widget == null)
                return NotFound();
            return Ok(widget);
        }

        // POST: api/widgets
        [HttpPost]
        public async Task<IActionResult> CreateWidget(Widget widget)
        {
            widget.IsActive = true;
            widget.IsDeleted = false;
            widget.CreatedAt = System.DateTime.UtcNow;
            widget.ModifiedAt = System.DateTime.UtcNow;

            _context.Widgets.Add(widget);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWidget), new { id = widget.Id }, widget);
        }

        // PUT: api/widgets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditWidget(int id, Widget widget)
        {
            if (id != widget.Id)
                return BadRequest();

            var existing = await _context.Widgets.FindAsync(id);
            if (existing == null || existing.IsDeleted)
                return NotFound();

            existing.Key = widget.Key;
            existing.Subject = widget.Subject;
            existing.ModifiedAt = System.DateTime.UtcNow;
            existing.ModifiedBy = widget.ModifiedBy;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // DELETE: api/wordcards/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWidget(int id)
        {
            var card = await _context.Widgets.FindAsync(id);
            if (card == null)
                return NotFound();
            card.IsDeleted = true;
            card.IsActive = false;
            _context.Entry(card).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Widget deleted successfully" });
        }
    }
}
