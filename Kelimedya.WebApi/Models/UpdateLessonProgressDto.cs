namespace Kelimedya.WebAPI.Models
{
    public class UpdateLessonProgressDto
    {
        public string StudentId { get; set; }
        public int LessonId { get; set; }
        public int LearnedWordCardsCount { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int TotalAttempts { get; set; }
        public double TotalTimeSpentSeconds { get; set; }
    }
}