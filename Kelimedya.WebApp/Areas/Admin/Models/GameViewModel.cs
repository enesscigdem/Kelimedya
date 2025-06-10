namespace Kelimedya.WebApp.Areas.Admin.Models;

public class GameViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? GameType { get; set; }
    public string? Difficulty { get; set; }
    public bool IsActive { get; set; }
}
