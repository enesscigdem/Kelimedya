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
    public class SalesController : ControllerBase
    {
        private readonly ParagraphDbContext _context;
        public SalesController(ParagraphDbContext context)
        {
            _context = context;
        }

        // GET: api/sales/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSalesSummary()
        {
            var totalSales = await _context.Orders
                .Where(o => !o.IsDeleted && o.IsActive)
                .SumAsync(o => o.TotalAmount);
            var orderCount = await _context.Orders
                .Where(o => !o.IsDeleted && o.IsActive)
                .CountAsync();
            return Ok(new { TotalSales = totalSales, OrderCount = orderCount });
        }
    }
}