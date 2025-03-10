using Paragraph.WebApp.Areas.Admin.Models;

namespace Paragraph.WebApp.Areas.Student.Models
{
    public class LessonWithProgressViewModel
    {
        public LessonViewModel Lesson { get; set; }
        public StudentLessonProgressViewModel Progress { get; set; }
    }
}