using System;

namespace Paragraph.WebAPI.Models
{
    public class LessonReportDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public string CourseTitle { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastAccessDate { get; set; }
    }
}