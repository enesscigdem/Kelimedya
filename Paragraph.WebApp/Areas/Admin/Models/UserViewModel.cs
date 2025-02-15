using System;
using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Kullanıcı ID gereklidir.")]
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Tam isim bilgisi gereklidir.")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Rol bilgisi gereklidir.")]
        public string Role { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}