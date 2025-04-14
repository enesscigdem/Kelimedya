using System;
using System.Collections.Generic;

namespace Kelimedya.WebAPI.Models
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; }
        public int TotalStudents { get; set; }
        public int NewStudents { get; set; }
        public decimal AverageProgress { get; set; }
        public List<StudentReportViewModel> StudentReports { get; set; }
    }
}