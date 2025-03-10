using System;

namespace Paragraph.WebApp.Areas.Student.Models
{
    public class StudentWordCardProgressViewModel
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public int WordCardId { get; set; }
        public int LessonId { get; set; }
        public bool IsLearned { get; set; }
        public bool IsMarkedForReview { get; set; }
        public int ViewCount { get; set; }
        public int CorrectAnswerCount { get; set; }
        public int WrongAnswerCount { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime FirstSeenDate { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public DateTime? LearnedDate { get; set; }
    }
}