using System;
using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities
{
    public class CartItem : IIntEntity, IAuditEntity, IIsDeletedEntity
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}