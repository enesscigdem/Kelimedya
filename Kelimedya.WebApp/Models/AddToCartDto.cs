using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class AddToCartDto
    {
        public string UserId { get; set; } = string.Empty;
        
        public int ProductId { get; set; }
        
        public int Quantity { get; set; }
    }
}