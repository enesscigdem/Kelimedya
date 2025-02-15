using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class LessonAggregateViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ders başlığı gereklidir.")]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Ders sırası gereklidir.")]
        [Range(1, 100, ErrorMessage = "Ders sırası 1 ile 100 arasında olmalıdır.")]
        public int SequenceNo { get; set; }
        
        // Alt kelime kartları
        public List<WordCardViewModel> WordCards { get; set; } = new();
    }
}