using Kelimedya.Core.BaseModels;
using System;

namespace Kelimedya.Core.Entities
{
    public class StudentLessonProgress : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public virtual Lesson Lesson { get; set; }

        public int LearnedWordCardsCount { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastAccessDate { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalAttempts { get; set; }
        public double TotalTimeSpentSeconds { get; set; }
    }
}