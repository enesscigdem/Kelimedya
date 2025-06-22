namespace Kelimedya.WebApp.Areas.Admin.Models;

public class TestQuestionViewModel
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public int CorrectOption { get; set; }
}
