using System;
using System.Collections.Generic;

namespace Paragraph.WebAPI.Models
{
    public class StudentReportDto
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public List<LessonReportDto> CompletedLessons { get; set; }
        public List<WordReportDto> LearnedWords { get; set; }
    }

    public class LessonReportDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public string CourseTitle { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastAccessDate { get; set; }
    }

    public class WordReportDto
    {
        public int WordCardId { get; set; }
        public string Word { get; set; }
        public string Definition { get; set; }
        public string ExampleSentence { get; set; }
        public int ViewCount { get; set; }
    }

    public class CoursePerformanceDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public decimal AverageCompletion { get; set; }
        public int TotalAttempts { get; set; }
        public double TotalTimeSpentSeconds { get; set; }
    }
}