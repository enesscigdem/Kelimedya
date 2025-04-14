using System;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class StudentLessonProgressViewModel
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public int CurrentWordCardIndex { get; set; }
        public int TotalWordCards { get; set; }
        public int LearnedWordCardsCount { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastAccessDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}