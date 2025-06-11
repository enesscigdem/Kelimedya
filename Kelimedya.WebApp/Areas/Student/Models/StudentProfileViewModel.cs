using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Areas.Student.Models;

public class StudentProfileViewModel
{
    public string Id { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? PhoneNumber { get; set; }
    public int? ClassGrade { get; set; }
}

