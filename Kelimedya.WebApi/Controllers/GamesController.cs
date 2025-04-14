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
    public class GamesController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public GamesController(KelimedyaDbContext context)
        {
            _context = context;
        }

        // GET: api/games
        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            var games = await _context.Games.ToListAsync();
            return Ok(games);
        }

        // GET: api/games/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return NotFound();
            return Ok(game);
        }

        // POST: api/games
        [HttpPost]
        public async Task<IActionResult> CreateGame(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
        }

        // PUT: api/games/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(int id, Game game)
        {
            if (id != game.Id)
                return BadRequest();
            _context.Entry(game).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Games.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/games/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return NotFound();
            game.IsDeleted = true;
            game.IsActive = false;
            _context.Entry(game).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Game deleted successfully" });
        }
    }
}
