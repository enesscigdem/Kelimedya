using System;
using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Areas.Admin.Models
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
        [Display(Name = "Mentor Öğretmen")]
        public int? TeacherId { get; set; }
        [Required(ErrorMessage = "Telefon numarası gereklidir.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Sınıf bilgisi gereklidir.")]
        [Range(0,12, ErrorMessage = "Sınıf 1 ile 12 arasında olmalıdır.")]
        public int? ClassGrade { get; set; }
    }
}