// Core/Entities/Coupon.cs

using Kelimedya.Core.BaseModels;
using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities
{
    public enum DiscountType : byte
    {
        Percentage = 0,
        FixedAmount = 1
    }

    public class Coupon : IIntEntity, IAuditEntity, IIsDeletedEntity
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }

        public int? ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}