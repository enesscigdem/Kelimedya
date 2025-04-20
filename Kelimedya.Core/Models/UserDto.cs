using System;
using System.Collections.Generic;

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
    }
}