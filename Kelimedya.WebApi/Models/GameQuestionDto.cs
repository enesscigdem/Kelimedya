namespace Kelimedya.WebAPI.Models;

public class GameQuestionDto
{
    public int GameId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
