using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Core.Enum;
using Kelimedya.Core.Models;
using System.Linq;
using System.Threading.Tasks;
using Kelimedya.Persistence;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public DashboardController(KelimedyaDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            int totalCourses = await _context.Courses.CountAsync(c => c.IsActive && !c.IsDeleted);
            int totalLessons = await _context.Lessons.CountAsync(l => l.IsActive && !l.IsDeleted);
            int totalWordCards = await _context.WordCards.CountAsync(w => w.IsActive && !w.IsDeleted);

            int totalOrders = await _context.Orders.CountAsync(o => o.IsActive && !o.IsDeleted);
            decimal totalRevenue = await _context.Orders
                .Where(o => o.IsActive && !o.IsDeleted && o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
            int educationSales = await _context.Orders
                .CountAsync(o => o.IsActive && !o.IsDeleted && o.Status == OrderStatus.Completed);
            int pendingPayments = await _context.Orders
                .CountAsync(o => o.IsActive && !o.IsDeleted && o.Status == OrderStatus.Pending);

            int incomingMessages = await _context.Messages
                .CountAsync(m => m.IsActive && !m.IsDeleted && !m.IsRead);
            int totalMessagesAll = await _context.Messages.CountAsync(m => m.IsActive && !m.IsDeleted);

            int totalUsers = await _context.Users.CountAsync();

            int totalRoles = await _context.Roles.CountAsync();
            int totalProducts = await _context.Products.CountAsync(p => p.IsActive && !p.IsDeleted);
            int totalReports = await _context.Reports.CountAsync(r => r.IsActive && !r.IsDeleted);

            var lastOrders = await _context.Orders
                .Where(o => o.IsActive && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            var dashboardDto = new DashboardDto
            {
                TotalCourses = totalCourses,
                TotalLessons = totalLessons,
                TotalWordCards = totalWordCards,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                EducationSales = educationSales,
                PendingPayments = pendingPayments,
                IncomingMessages = incomingMessages,
                TotalUsers = totalUsers,
                TotalRoles = totalRoles,
                TotalProducts = totalProducts,
                TotalReports = totalReports,
                TotalMessagesAll = totalMessagesAll,
                LastOrders = lastOrders
            };

            return Ok(dashboardDto);
        }
    }
}
