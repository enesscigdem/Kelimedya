using System.Collections.Generic;
using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class LessonDetailViewModel
    {
        public LessonViewModel Lesson { get; set; }
        public List<WordCardWithProgressViewModel> WordCards { get; set; }
        public StudentLessonProgressViewModel LessonProgress { get; set; }
        // Öğrenilen kelimeler: IsLearned true olanlar
        public List<WordCardWithProgressViewModel> LearnedWords { get; set; } = new List<WordCardWithProgressViewModel>();
    }
}