using System;
using System.Collections.Generic;
using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities
{
    public class Cart : IIntEntity, IAuditEntity, IIsDeletedEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Kullanıcının Id'si (Identity kullanıyorsanız)
        public int? ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}