using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Eğitim başlığı gereklidir.")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ders sayısı en az 1 olmalıdır.")]
        public int LessonCount { get; set; } = 40;

        [Range(1, int.MaxValue, ErrorMessage = "Kelime sayısı en az 1 olmalıdır.")]
        public int WordCount { get; set; } = 400;

        public bool IsActive { get; set; } = true;
    }
}