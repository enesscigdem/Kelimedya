using Paragraph.WebApp.Areas.Admin.Models;

namespace Paragraph.WebApp.Areas.Student.Models
{
    public class CourseWithProgressViewModel
    {
        public CourseViewModel Course { get; set; }
        public StudentCourseProgressViewModel Progress { get; set; }
    }
}