using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Kelimedya.WebApp.Areas.Admin.Models;

public class AdminProfileViewModel
{
    public string Id { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePicture { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
