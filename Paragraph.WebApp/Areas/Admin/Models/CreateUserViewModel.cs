using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Ad gereklidir.")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Soyad gereklidir.")]
        public string Surname { get; set; }
        
        [Required(ErrorMessage = "Rol seçimi gereklidir.")]
        public string Role { get; set; }
    }
}