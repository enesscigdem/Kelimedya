namespace Kelimedya.WebApp.Areas.Admin.Models;

public class GameQuestionViewModel
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
