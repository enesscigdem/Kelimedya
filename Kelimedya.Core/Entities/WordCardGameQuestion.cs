using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities;

public class WordCardGameQuestion : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }

    public int WordCardId { get; set; }
    public int GameId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
