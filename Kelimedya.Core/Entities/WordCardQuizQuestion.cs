using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities;

public class WordCardQuizQuestion : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }

    public int WordCardId { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = string.Empty;
}
