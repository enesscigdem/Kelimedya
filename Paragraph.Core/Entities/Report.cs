using Paragraph.Core.BaseModels;
using Paragraph.Core.Entities;

namespace Paragraph.Core.Entities
{
    public class Report : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? ReportDate { get; set; }
    }
}