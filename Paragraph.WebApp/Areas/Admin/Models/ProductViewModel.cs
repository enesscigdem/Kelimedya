using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Açıklama gereklidir.")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Fiyat bilgisi gereklidir.")]
        public decimal Price { get; set; }
    }
}