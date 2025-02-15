using Paragraph.Core.BaseModels;

namespace Paragraph.Core.Entities
{
    public class WordCard : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public int LessonId { get; set; }

        public string Word { get; set; } = null!;            // Örn: "Adeta"
        public string Definition { get; set; } = null!;      // Kelimenin anlamı
        public string? ExampleSentence { get; set; }         // Örnek cümle
        // public string? ImageUrl { get; set; }
    }
}