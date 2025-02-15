using Paragraph.Core.BaseModels;
using System;

namespace Paragraph.Core.Entities
{
    public class Widget : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string Key { get; set; } = null!;
        public string Subject { get; set; } = null!;
    }
}