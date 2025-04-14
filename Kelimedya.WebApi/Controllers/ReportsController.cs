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
    public class ReportsController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public ReportsController(KelimedyaDbContext context)
        {
            _context = context;
        }

        // GET: api/reports
        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _context.Reports.ToListAsync();
            return Ok(reports);
        }

        // GET: api/reports/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound();
            return Ok(report);
        }

        // POST: api/reports
        [HttpPost]
        public async Task<IActionResult> CreateReport(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }

        // PUT: api/reports/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, Report report)
        {
            if (id != report.Id)
                return BadRequest();
            _context.Entry(report).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reports.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/reports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound();
            report.IsDeleted = true;
            report.IsActive = false;
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Report deleted successfully" });
        }
    }
}
