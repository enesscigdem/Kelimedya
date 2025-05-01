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
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .Include(c => c.Coupon)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (cart == null)
                return Ok(new CartDto());

            // Sepet ara toplam
            var subTotal = cart.Items.Sum(i => i.Quantity * i.Product.Price);

            // Eğer sepetin içinde bir kupon var ise geçerliliğini kontrol et
            if (cart.CouponId.HasValue)
            {
                var cp = cart.Coupon!;
                var now = DateTime.UtcNow;
                if (cp.IsDeleted || !cp.IsActive
                                 || (cp.ValidFrom.HasValue && cp.ValidFrom > now)
                                 || (cp.ValidTo.HasValue && cp.ValidTo < now))
                {
                    // geçersiz kuponu sil
                    cart.CouponId = null;
                    cart.CouponDiscount = 0;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // yeni indirim tutarını oku
                    var discount = cp.DiscountType == DiscountType.Percentage
                        ? subTotal * cp.DiscountValue / 100m
                        : cp.DiscountValue;
                    cart.CouponDiscount = Math.Min(discount, subTotal);
                    cart.ModifiedAt = now;
                    await _context.SaveChangesAsync();
                }
            }

            // DTO’ya dönüştür
            var dto = new CartDto
            {
                Id = cart.Id,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ImageUrl = i.Product.ImageUrl,
                    Price = i.Product.Price,
                    Quantity = i.Quantity
                }).ToList(),
                CouponCode = cart.Coupon?.Code,
                CouponDiscount = cart.CouponDiscount
            };
            return Ok(dto);
        }

        // POST: api/cart/applyCoupon
        [HttpPost("applyCoupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == dto.UserId && !c.IsDeleted);
            if (cart == null) return NotFound("Sepet bulunamadı.");

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c =>
                    c.Code == dto.CouponCode && !c.IsDeleted && c.IsActive
                    && (!c.ValidFrom.HasValue || c.ValidFrom <= DateTime.Now)
                    && (!c.ValidTo.HasValue || c.ValidTo >= DateTime.Now)
                );
            if (coupon == null)
                return BadRequest("Kupon bulunamadı veya geçerli değil.");

            var subTotal = cart.Items.Sum(i => i.Quantity * i.Product.Price);
            var discount = coupon.DiscountType == DiscountType.Percentage
                ? subTotal * coupon.DiscountValue / 100m
                : coupon.DiscountValue;
            discount = Math.Min(discount, subTotal);

            cart.CouponId = coupon.Id;
            cart.CouponDiscount = discount;
            cart.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                couponCode = coupon.Code,
                couponDiscountFormatted = discount.ToString("C"),
                cartSubtotalFormatted = subTotal.ToString("C"),
                cartTotalFormatted = (subTotal - discount).ToString("C")
            });
        }

        // POST: api/cart/removeCoupon
        [HttpPost("removeCoupon")]
        public async Task<IActionResult> RemoveCoupon([FromBody] ApplyCouponDto dto)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == dto.UserId && !c.IsDeleted);
            if (cart == null) return NotFound();

            cart.CouponId = null;
            cart.CouponDiscount = 0;
            cart.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok();
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

            return Ok(new
            {
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

            return Ok(new
            {
                cartSubtotalFormatted = cartSubtotal.ToString("C"),
                cartTotalFormatted = cartSubtotal.ToString("C")
            });
        }
    }
}