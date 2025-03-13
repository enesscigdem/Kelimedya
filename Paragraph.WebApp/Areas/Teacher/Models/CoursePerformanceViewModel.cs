namespace Paragraph.WebApp.Areas.Teacher.Models
{
    public class CoursePerformanceViewModel
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public decimal AverageCompletion { get; set; }
        public int TotalAttempts { get; set; }
        public double TotalTimeSpentSeconds { get; set; }
    }
}