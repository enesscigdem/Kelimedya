using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class LessonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Eğitim seçimi gereklidir.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Ders başlığı gereklidir.")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Ders sırası gereklidir.")]
        [Range(1, 100, ErrorMessage = "Ders sırası 1 ile 100 arasında olmalıdır.")]
        public int SequenceNo { get; set; }
    }
}