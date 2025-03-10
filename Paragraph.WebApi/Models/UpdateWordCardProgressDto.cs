namespace Paragraph.WebAPI.Models
{
    public class UpdateWordCardProgressDto
    {
        public string StudentId { get; set; }
        public int WordCardId { get; set; }
        public int LessonId { get; set; }
        public bool IsLearned { get; set; }
        // Ek istatistik: Cevap süresi (saniye)
        public double ResponseTimeSeconds { get; set; }
    }
}