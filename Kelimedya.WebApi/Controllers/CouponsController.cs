using Kelimedya.Core.Entities;
using Kelimedya.Persistence;
using Kelimedya.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        public CouponsController(KelimedyaDbContext context) => _context = context;

        // GET: api/coupons
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Coupons
                .Where(c => !c.IsDeleted)
                .Select(c => new CouponDto {
                    Id = c.Id,
                    Code = c.Code,
                    Description = c.Description,
                    DiscountType = c.DiscountType,
                    DiscountValue = c.DiscountValue,
                    CreatedAt = c.CreatedAt,
                    ValidFrom = c.ValidFrom,
                    ValidTo = c.ValidTo,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/coupons/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var c = await _context.Coupons.FindAsync(id);
            if (c == null || c.IsDeleted) return NotFound();

            return Ok(new CouponDto {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                ValidFrom = c.ValidFrom,
                ValidTo = c.ValidTo,
                IsActive = c.IsActive
            });
        }

        // POST: api/coupons
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            if (await _context.Coupons.AnyAsync(c => c.Code == dto.Code && !c.IsDeleted))
                return BadRequest("Bu kod zaten var.");

            var c = new Coupon
            {
                Code = dto.Code,
                Description = dto.Description,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            _context.Coupons.Add(c);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
        }

        // PUT: api/coupons/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponDto dto)
        {
            if (id != dto.Id) 
                return BadRequest("ID uyuşmuyor.");

            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var c = await _context.Coupons.FindAsync(id);
            if (c == null || c.IsDeleted) 
                return NotFound();

            c.Code           = dto.Code;
            c.Description    = dto.Description;
            c.DiscountType   = dto.DiscountType;
            c.DiscountValue  = dto.DiscountValue;
            c.ValidFrom      = dto.ValidFrom;
            c.ValidTo        = dto.ValidTo;
            c.IsActive       = dto.IsActive;
            c.ModifiedAt     = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/coupons/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Coupons.FindAsync(id);
            if (c == null || c.IsDeleted) 
                return NotFound();

            c.IsDeleted   = true;
            c.IsActive    = false;
            _context.Entry(c).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
