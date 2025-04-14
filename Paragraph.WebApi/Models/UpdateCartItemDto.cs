using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebAPI.Models
{
    public class UpdateCartItemDto
    {
        public int ItemId { get; set; }
        
        public int Quantity { get; set; }
    }
}
