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
    public class MessagesController : ControllerBase
    {
        private readonly ParagraphDbContext _context;
        public MessagesController(ParagraphDbContext context)
        {
            _context = context;
        }

        // GET: api/messages
        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages.ToListAsync();
            return Ok(messages);
        }

        // GET: api/messages/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        // POST: api/messages
        [HttpPost]
        public async Task<IActionResult> CreateMessage(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }

        // PUT: api/messages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, Message message)
        {
            if (id != message.Id)
                return BadRequest();
            _context.Entry(message).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Messages.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/messages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            message.IsDeleted = true;
            message.IsActive = false;
            _context.Entry(message).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Message deleted successfully" });
        }
    }
}
