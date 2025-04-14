using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class LessonWithProgressViewModel
    {
        public LessonViewModel Lesson { get; set; }
        public StudentLessonProgressViewModel Progress { get; set; }
    }
}