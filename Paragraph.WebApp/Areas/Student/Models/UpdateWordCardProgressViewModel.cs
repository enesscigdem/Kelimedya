namespace Paragraph.WebApp.Areas.Student.Models
{
    public class UpdateWordCardProgressViewModel
    {
        public int WordCardId { get; set; }
        public bool IsLearned { get; set; }
        public bool IsMarkedForReview { get; set; }
    }
}