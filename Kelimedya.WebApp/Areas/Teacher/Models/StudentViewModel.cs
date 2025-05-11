using System;

namespace Kelimedya.WebApp.Areas.Teacher.Models
{
    public class StudentViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? ClassGrade { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}