namespace Kelimedya.WebApp.Models;

public class UserInfoViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string Area { get; set; } = string.Empty;
}
