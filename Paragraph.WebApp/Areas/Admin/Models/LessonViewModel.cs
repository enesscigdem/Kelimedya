using System.ComponentModel.DataAnnotations;
using Paragraph.Core.Entities;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class LessonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Eğitim seçimi gereklidir.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Ders başlığı gereklidir.")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Ders sırası gereklidir.")]
        [Range(1, 100, ErrorMessage = "Ders sırası 1 ile 100 arasında olmalıdır.")]
        public int SequenceNo { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int WordCount { get; set; }
        public List<WordCard> WordCards { get; set; } = new List<WordCard>();
        public int GameCount { get; set; }
        public bool IsStarted { get; set; }
    }
}