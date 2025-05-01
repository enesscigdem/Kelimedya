using System.ComponentModel.DataAnnotations;
using Kelimedya.Core.Entities;

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}
public class CreateCouponDto
{
    [Required][StringLength(50)] public string Code { get; set; }
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    [Range(0,999999)] public decimal DiscountValue { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}
public class UpdateCouponDto : CreateCouponDto
{
    [Required] public int Id { get; set; }
}