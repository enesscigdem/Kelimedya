namespace Kelimedya.WebApp.Areas.Teacher.Models
{
    public class TeacherOverviewViewModel
    {
        public int TotalStudents { get; set; }
        public double AvgLessonCompletion { get; set; }
        public int TotalLearnedWords { get; set; }
        public int TotalQuizAttempts { get; set; }
        public double AvgQuizScore { get; set; }
        public int TotalGamePoints { get; set; }
    }
}