namespace Kelimedya.WebApp.Areas.Admin.Models;

public class GameQuestionViewModel
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public string? QuestionType { get; set; }
    public string? OptionA { get; set; }
    public string? OptionB { get; set; }
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
    public int? CorrectOption { get; set; }
}
