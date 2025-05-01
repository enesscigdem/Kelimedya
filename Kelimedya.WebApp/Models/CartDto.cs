using System.Collections.Generic;
using System.Linq;

namespace Kelimedya.WebApp.Models
{
    public class CartDto
    {
        public int Id { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public string? CouponCode    { get; set; }
        public decimal CouponDiscount{ get; set; } = 0m;

        public decimal SubTotal => Items.Sum(i => i.Total);
        public decimal Total    => SubTotal - CouponDiscount;
    }
}