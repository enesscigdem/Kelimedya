using System.Collections.Generic;
using System.Linq;

namespace Kelimedya.WebApp.Models
{
    public class CartDto
    {
        public int Id { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalPrice => Items.Sum(i => i.Total);
    }
}