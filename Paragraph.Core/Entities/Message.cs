using Paragraph.Core.BaseModels;

namespace Paragraph.Core.Entities
{
    public class Message : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public bool IsRead { get; set; }
    }
}