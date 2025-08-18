using System.Collections.Generic;

namespace Kelimedya.WebApp.Areas.Teacher.Models
{
    public class StudentReportsPageViewModel
    {
        public List<StudentReportViewModel> Students { get; set; } = new();
        public TeacherOverviewViewModel Overview { get; set; } = new();
    }
}