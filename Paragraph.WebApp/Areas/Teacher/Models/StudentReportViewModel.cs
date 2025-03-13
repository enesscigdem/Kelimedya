using System;
using System.Collections.Generic;

namespace Paragraph.WebApp.Areas.Teacher.Models
{
    public class StudentReportViewModel
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public List<LessonReportViewModel> CompletedLessons { get; set; } = new List<LessonReportViewModel>();
        public List<WordReportViewModel> LearnedWords { get; set; } = new List<WordReportViewModel>();
    }

    public class LessonReportViewModel
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public string CourseTitle { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastAccessDate { get; set; }
    }

    public class WordReportViewModel
    {
        public int WordCardId { get; set; }
        public string Word { get; set; }
        public string Definition { get; set; }
        public string ExampleSentence { get; set; }
        public int ViewCount { get; set; }
    }
}