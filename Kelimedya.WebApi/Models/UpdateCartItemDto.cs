using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebAPI.Models
{
    public class UpdateCartItemDto
    {
        public int ItemId { get; set; }
        
        public int Quantity { get; set; }
    }
}
