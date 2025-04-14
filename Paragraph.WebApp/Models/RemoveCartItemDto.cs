using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Models
{
    public class RemoveCartItemDto
    {
        [Required]
        public int ItemId { get; set; }
    }
}