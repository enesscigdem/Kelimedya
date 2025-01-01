namespace Paragraph.Core.Models;
public class AuthResultViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();
}
