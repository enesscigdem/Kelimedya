namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class UpdateWordCardProgressViewModel
    {
        public int WordCardId { get; set; }
        public int LessonId { get; set; }
        public bool IsLearned { get; set; }
        public bool IsMarkedForReview { get; set; }
        public double ResponseTimeSeconds { get; set; }
    }
}