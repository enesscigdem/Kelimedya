using Paragraph.WebApp.Areas.Admin.Models;

namespace Paragraph.WebApp.Areas.Student.Models
{
    public class WordCardWithProgressViewModel
    {
        public WordCardViewModel WordCard { get; set; }
        public StudentWordCardProgressViewModel Progress { get; set; }
    }
}