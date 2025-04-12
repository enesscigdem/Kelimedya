using System;
using System.Collections.Generic;

namespace Paragraph.WebAPI.Models
{
    public class StudentReportViewModel
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LessonReportDto> CompletedLessons { get; set; }
    }
}