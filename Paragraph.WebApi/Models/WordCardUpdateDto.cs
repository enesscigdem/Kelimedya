using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebAPI.Models
{
    public class WordCardUpdateDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public int LessonId { get; set; }
        
        [Required(ErrorMessage = "Kelime alanı gereklidir.")]
        public string Word { get; set; }
        
        [Required(ErrorMessage = "Tanım alanı gereklidir.")]
        public string Definition { get; set; }
        
        public string? ExampleSentence { get; set; }
        
        public IFormFile? ImageFile { get; set; }
    }
}