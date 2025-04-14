using Kelimedya.WebApp.Areas.Admin.Models;

namespace Kelimedya.WebApp.Areas.Student.Models
{
    public class WordCardWithProgressViewModel
    {
        public WordCardViewModel WordCard { get; set; }
        public StudentWordCardProgressViewModel Progress { get; set; }
    }
}