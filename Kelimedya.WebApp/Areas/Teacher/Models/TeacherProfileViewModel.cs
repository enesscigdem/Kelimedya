using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Kelimedya.WebApp.Areas.Teacher.Models;

public class TeacherProfileViewModel
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
}
