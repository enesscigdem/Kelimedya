using System.Collections.Generic;

namespace Paragraph.WebApp.Areas.Student.Models
{
    public class MyWordsViewModel
    {
        public List<WordCardWithProgressViewModel> LearnedWords { get; set; } = new List<WordCardWithProgressViewModel>();
    }
}