using System.Collections.Generic;
using Paragraph.WebApp.Areas.Admin.Models;

namespace Paragraph.WebApp.Areas.Student.Models
{
    public class CoursePlayViewModel
    {
        public CourseViewModel Course { get; set; }
        // Ders bilgilerini, ilerleme bilgileriyle birlikte almak için LessonWithProgressViewModel kullanalım:
        public List<LessonWithProgressViewModel> Lessons { get; set; }
    }
}