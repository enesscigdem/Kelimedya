using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        [Display(Name = "Mentor Öğretmen")] public int? TeacherId { get; set; }
        public List<SelectListItem> Teachers { get; set; } = new();

        [Required(ErrorMessage = "Telefon numarası gereklidir.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Sınıf bilgisi gereklidir.")]
        [Range(0, 12, ErrorMessage = "Sınıf 0 ile 12 arasında olmalıdır.")]
        public int? ClassGrade { get; set; }
    }
}