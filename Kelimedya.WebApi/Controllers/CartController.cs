using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.WebAPI.Models;
using Kelimedya.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;

        public CartController(KelimedyaDbContext context)
        {
            _context = context;
        }

        // GET: api/cart/{userId} 
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (cart == null)
                return Ok(new CartDto()); // Boş sepet dön

            var cartDto = new CartDto
            {
                Id = cart.Id,
                Items = cart.Items.Select(item => new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ImageUrl = item.Product.ImageUrl,
                    Price = item.Product.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            return Ok(cartDto);
        }

        // POST: api/cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kullanıcıya ait aktif sepet varsa onu getir, yoksa yeni oluştur.
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == dto.UserId && !c.IsDeleted);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = dto.UserId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Aynı ürün daha önce eklenmişse miktarı güncelle, yoksa yeni ekle.
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId && !i.IsDeleted);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                existingItem.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
            cart.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }
        // POST: api/cart/updateQuantity
        [HttpPost("updateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ürünle ilgili bilgiyi içeren CartItem'ı getiriyoruz.
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == dto.ItemId && !ci.IsDeleted);

            if (cartItem == null)
                return NotFound(new { Message = "Sepet öğesi bulunamadı." });

            cartItem.Quantity = dto.Quantity;
            cartItem.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // İlgili sepetin (Cart) güncel toplamlarını hesaplayalım.
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartItem.CartId && !ci.IsDeleted)
                .ToListAsync();

            decimal cartSubtotal = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);
            
            return Ok(new {
                itemTotalFormatted = (cartItem.Quantity * cartItem.Product.Price).ToString("C"),
                cartSubtotalFormatted = cartSubtotal.ToString("C"),
                cartTotalFormatted = cartSubtotal.ToString("C")
            });
        }
        // POST: api/cart/remove
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveItemFromPost([FromForm] int itemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && !ci.IsDeleted);

            if (cartItem == null)
                return NotFound(new { Message = "Sepet öğesi bulunamadı." });

            // Soft delete: ilgili öğeyi işaretleyip güncelliyoruz.
            cartItem.IsDeleted = true;
            cartItem.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Güncel sepet toplamını hesaplayalım.
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartItem.CartId && !ci.IsDeleted)
                .ToListAsync();

            decimal cartSubtotal = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            return Ok(new {
                cartSubtotalFormatted = cartSubtotal.ToString("C"),
                cartTotalFormatted = cartSubtotal.ToString("C")
            });
        }

    }
}
