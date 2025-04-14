using System.Collections.Generic;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class MyWordsViewModel
    {
        public List<WordCardWithProgressViewModel> LearnedWords { get; set; } = new List<WordCardWithProgressViewModel>();
    }
}