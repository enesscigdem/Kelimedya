using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities;

public class StudentQuizResult : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public int? LessonId { get; set; }
    public int? CourseId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int Score { get; set; }
    public DateTime CompletedAt { get; set; }
}
