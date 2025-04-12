using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebAPI.Models
{
    public class CourseCreateDto
    {
        [Required]
        public int CourseId { get; set; }
        
        [Required(ErrorMessage = "Ders başlığı gereklidir.")]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        public IFormFile? ImageFile { get; set; }
    }
}