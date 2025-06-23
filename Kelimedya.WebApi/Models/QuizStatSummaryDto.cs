using System;

namespace Kelimedya.WebAPI.Models
{
    public class QuizStatSummaryDto
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public double AverageScore { get; set; }
    }
}
