using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebAPI.Models
{
    public class LessonUpdateDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required(ErrorMessage = "Ders başlığı gereklidir.")]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Ders sırası gereklidir.")]
        [Range(1, 100, ErrorMessage = "Ders sırası 1 ile 100 arasında olmalıdır.")]
        public int SequenceNo { get; set; }
        
        public IFormFile? ImageFile { get; set; }
    }
}