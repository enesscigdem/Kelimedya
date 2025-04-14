using System.Collections.Generic;

namespace Kelimedya.WebAPI.Models
{
    public class StudentDashboardDto
    {
        public int TotalCourses { get; set; }
        public int CompletedLessons { get; set; }
        public int LearnedWords { get; set; }
        public List<CourseProgressDto> CourseProgresses { get; set; } = new List<CourseProgressDto>();
        public List<LessonProgressDto> LessonProgresses { get; set; } = new List<LessonProgressDto>();
    }

    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public decimal AverageCompletion { get; set; }
    }

    public class LessonProgressDto
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; }
        public decimal CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
    }
}