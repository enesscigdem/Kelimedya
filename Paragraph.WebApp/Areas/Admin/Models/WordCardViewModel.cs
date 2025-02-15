using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class WordCardViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ders seçimi gereklidir.")]
        public int LessonId { get; set; }
        
        [Required(ErrorMessage = "Kelime alanı gereklidir.")]
        public string Word { get; set; }
        
        [Required(ErrorMessage = "Tanım alanı gereklidir.")]
        public string Definition { get; set; }
        
        public string? ExampleSentence { get; set; }
    }
}