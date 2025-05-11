using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kelimedya.Core.Models
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? TeacherId { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }

        [Range(0, 12)]
        public int? ClassGrade { get; set; }
    }

    public class CreateUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }        
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Role { get; set; }
        public int? TeacherId { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }

        [Range(0, 12)]
        public int? ClassGrade { get; set; }
    }

    public class UpdateUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }        
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Role { get; set; }
        public int? TeacherId { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }

        [Range(0, 12)]
        public int? ClassGrade { get; set; }
    }
}