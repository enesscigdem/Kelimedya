using System.Collections.Generic;
using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class MyLessonsViewModel
    {
        public CourseViewModel Course { get; set; }
        // Ders bilgilerini, ilerleme bilgileriyle birlikte almak için LessonWithProgressViewModel kullanalım:
        public List<LessonWithProgressViewModel> Lessons { get; set; }
    }
}