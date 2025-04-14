using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class UpdateCartItemDto
    {
        public int ItemId { get; set; }

        public int Quantity { get; set; }
    }
}