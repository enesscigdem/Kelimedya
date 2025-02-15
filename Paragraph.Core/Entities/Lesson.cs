using Paragraph.Core.BaseModels;

namespace Paragraph.Core.Entities
{
    public class Lesson : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public int CourseId { get; set; }           // Hangi kursa ait
        public string Title { get; set; } = null!;  // Örnek: "Ders 1"
        public string? Description { get; set; }
        
        // Ders sırası
        public int SequenceNo { get; set; }         // Örneğin 1,2,3... 40
    }
}