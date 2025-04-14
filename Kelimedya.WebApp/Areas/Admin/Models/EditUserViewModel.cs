using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Areas.Admin.Models
{
    public class EditUserViewModel
    {
        [Required(ErrorMessage = "Kullanıcı ID gereklidir.")]
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Ad gereklidir.")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Soyad gereklidir.")]
        public string Surname { get; set; }
        
        [Required(ErrorMessage = "Rol seçimi gereklidir.")]
        public string Role { get; set; }
    }
}