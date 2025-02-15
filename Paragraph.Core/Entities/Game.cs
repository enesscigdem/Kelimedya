using Paragraph.Core.BaseModels;

namespace Paragraph.Core.Entities
{
    public class Game : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string Title { get; set; } = null!;
        public string? GameType { get; set; }   // Çoktan Seçmeli, Adam Asmaca vb.
        public string? Difficulty { get; set; } // Kolay/Orta/Zor vb.
        
        // public int? LessonId { get; set; }
    }
}