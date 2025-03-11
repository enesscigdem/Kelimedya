using System.Collections.Generic;

namespace Paragraph.WebAPI.Models
{
    public class StudentDashboardDto
    {
        public int TotalCourses { get; set; }         // İlerlemesi olan farklı kurs sayısı
        public int CompletedLessons { get; set; }       // Tamamlanmış ders sayısı
        public int LearnedWords { get; set; }           // Öğrenilmiş kelime sayısı
        public List<CourseProgressDto> CourseProgresses { get; set; } = new List<CourseProgressDto>();
        public List<LessonProgressDto> LessonProgresses { get; set; } = new List<LessonProgressDto>(); // Yeni eklenen ders ilerlemeleri listesi
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