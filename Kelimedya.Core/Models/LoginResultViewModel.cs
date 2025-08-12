namespace Kelimedya.Core.Models;

public class LoginResultViewModel
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? Role { get; set; }
}
