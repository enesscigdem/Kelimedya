using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities;

public class StudentGameStatistic : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public int Score { get; set; }
    public double DurationSeconds { get; set; }
    public DateTime PlayedAt { get; set; }
}
