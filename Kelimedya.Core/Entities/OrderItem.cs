using System.Text.Json.Serialization;
using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities;

public class OrderItem : IIntEntity, IAuditEntity, IIsDeletedEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    [JsonIgnore]
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public int? ModifiedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
}

