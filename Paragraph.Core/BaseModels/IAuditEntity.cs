namespace Paragraph.Core.BaseModels;

public interface IAuditEntity
{
    public DateTime ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
}