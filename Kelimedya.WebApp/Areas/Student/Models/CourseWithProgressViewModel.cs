using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class CourseWithProgressViewModel
    {
        public CourseViewModel Course { get; set; }
        public StudentCourseProgressViewModel Progress { get; set; }
    }
}