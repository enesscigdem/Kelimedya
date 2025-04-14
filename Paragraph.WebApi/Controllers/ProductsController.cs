using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paragraph.Core.Entities;
using Paragraph.WebAPI.Models;
using Paragraph.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Paragraph.WebAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ParagraphDbContext _context;
        public ProductsController(ParagraphDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.ProductCourses)
                    .ThenInclude(pc => pc.Course)
                .ToListAsync();
            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCourses)
                    .ThenInclude(pc => pc.Course)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            // Fotoğraf dosyası varsa kaydet
            if (dto.ImageFile != null)
            {
                product.ImageUrl = await SaveFileAsync(dto.ImageFile, "products");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Ürünle ilişkilendirilen eğitimleri ekle (join table)
            if (dto.CourseIds != null && dto.CourseIds.Any())
            {
                foreach(var courseId in dto.CourseIds)
                {
                    _context.Add(new ProductCourse { ProductId = product.Id, CourseId = courseId });
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductCourses)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.ModifiedAt = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {
                // Eski dosyayı silmek için kod eklenebilir.
                product.ImageUrl = await SaveFileAsync(dto.ImageFile, "products");
            }

            // Ürün ile ilişkilendirilen kursları güncelleyelim.
            // Öncelikle mevcut ilişki kayıtlarını kaldırıyoruz.
            _context.RemoveRange(product.ProductCourses);

            if (dto.CourseIds != null && dto.CourseIds.Any())
            {
                foreach(var courseId in dto.CourseIds)
                {
                    product.ProductCourses.Add(new ProductCourse { ProductId = product.Id, CourseId = courseId });
                }
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            // Soft delete: işaretleyip güncelliyoruz.
            product.IsDeleted = true;
            product.IsActive = false;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product deleted successfully" });
        }

        // Yardımcı metod: Dosya kaydetme
        private async Task<string?> SaveFileAsync(Microsoft.AspNetCore.Http.IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"{Request.Scheme}://{Request.Host}/uploads/{folder}/{fileName}";
        }
    }
}
