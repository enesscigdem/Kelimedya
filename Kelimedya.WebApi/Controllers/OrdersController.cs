using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using Microsoft.AspNetCore.Authorization;
using Kelimedya.Core.Enum;
using System.Linq;
using System.Threading.Tasks;
using Kelimedya.Core.Enum;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public OrdersController(KelimedyaDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();
            return Ok(orders);
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // POST: api/orders/checkout/{userId}
        [HttpPost("checkout/{userId}")]
        [Authorize]
        public async Task<IActionResult> Checkout(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                    .ThenInclude(p => p.ProductCourses)
                .ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (cart == null || !cart.Items.Any())
                return BadRequest("Sepet boş.");

            var order = new Order
            {
                IsActive = true,
                IsDeleted = false,
                OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 8),
                OrderDate = DateTime.UtcNow,
                CustomerName = userId,
                CustomerEmail = null,
                SubTotal = cart.Items.Sum(i => i.Quantity * i.Product.Price),
                DiscountAmount = cart.CouponDiscount,
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.Product.Price) - cart.CouponDiscount,
                CouponCode = cart.Coupon?.Code,
                UserId = userId,
                Status = OrderStatus.Completed
            };

            foreach (var item in cart.Items.Where(i => !i.IsDeleted))
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                });

                foreach (var pc in item.Product.ProductCourses)
                {
                    var lessons = await _context.Lessons
                        .Where(l => l.CourseId == pc.CourseId && !l.IsDeleted)
                        .ToListAsync();
                    foreach (var lesson in lessons)
                    {
                        if (!await _context.StudentLessonProgresses.AnyAsync(p => p.StudentId == userId && p.LessonId == lesson.Id))
                        {
                            _context.StudentLessonProgresses.Add(new StudentLessonProgress
                            {
                                StudentId = userId,
                                LessonId = lesson.Id,
                                StartDate = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow,
                                ModifiedAt = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                    }
                }
            }

            _context.Orders.Add(order);

            // Sepeti temizle
            cart.Items.Clear();
            cart.CouponId = null;
            cart.CouponDiscount = 0;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // PUT: api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.Id)
                return BadRequest();
            _context.Entry(order).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();
            order.IsDeleted = true;
            order.IsActive = false;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Order deleted successfully" });
        }

        // GET: api/orders/courses/{studentId}
        [HttpGet("courses/{studentId}")]
        public async Task<IActionResult> GetStudentCourses(string studentId)
        {
            var courses = await _context.Orders
                .Where(o => o.UserId == studentId && !o.IsDeleted)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductCourses)
                            .ThenInclude(pc => pc.Course)
                .SelectMany(o => o.Items)
                .SelectMany(i => i.Product.ProductCourses)
                .Select(pc => pc.Course)
                .Distinct()
                .ToListAsync();

            return Ok(courses);
        }
    }
}
