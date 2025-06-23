using System.Text.Json.Serialization;

namespace Kelimedya.WebAPI.Models;

public class RecordQuizResultDto
{
    public string StudentId { get; set; } = string.Empty;
    public int? LessonId { get; set; }
    public int? CourseId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int Score { get; set; }
}
