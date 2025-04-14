using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class RemoveCartItemDto
    {
        [Required]
        public int ItemId { get; set; }
    }
}