using Microsoft.AspNetCore.Http;
namespace Kelimedya.WebAPI.Models;

public class GameQuestionDto
{
    public int GameId { get; set; }
    public string? QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrl2 { get; set; }
    public string? ImageUrl3 { get; set; }
    public string? ImageUrl4 { get; set; }

    public IFormFile? ImageFile { get; set; }
    public IFormFile? ImageFile2 { get; set; }
    public IFormFile? ImageFile3 { get; set; }
    public IFormFile? ImageFile4 { get; set; }

    public string? QuestionType { get; set; }
    public string? OptionA { get; set; }
    public string? OptionB { get; set; }
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
    public int? CorrectOption { get; set; }
}
