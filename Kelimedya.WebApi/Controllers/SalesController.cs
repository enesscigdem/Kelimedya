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
    public class SalesController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public SalesController(KelimedyaDbContext context)
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