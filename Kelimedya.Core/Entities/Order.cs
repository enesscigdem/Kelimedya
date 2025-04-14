using Kelimedya.Core.BaseModels;
using Kelimedya.Core.Entities;
using Kelimedya.Core.Enum;

namespace Kelimedya.Core.Entities
{
    public class Order : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string OrderNumber { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }
    }
}