using System.Collections.Generic;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class StudentDashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int CompletedLessons { get; set; }
        public int LearnedWords { get; set; }
        public List<CourseProgressViewModel> CourseProgresses { get; set; } = new List<CourseProgressViewModel>();
        public List<LessonProgressViewModel> LessonProgresses { get; set; } = new List<LessonProgressViewModel>();
    }

    public class CourseProgressViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public decimal AverageCompletion { get; set; }
    }

    public class LessonProgressViewModel
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; }
        public decimal CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
    }
}