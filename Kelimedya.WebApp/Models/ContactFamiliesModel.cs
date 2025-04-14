using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class ContactFamiliesModel
    {
        [Required(ErrorMessage = "Lütfen adınızı giriniz.")]
        public string Name { get; set; }

        // E-posta için property adı "FromEmail" olarak kalabilir; bu durumda formda name="FromEmail" kullanılmalı.
        [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string FromEmail { get; set; }

        [Required(ErrorMessage = "Lütfen mesajınızı giriniz.")]
        public string Body { get; set; }
        
        public string Subject { get; set; }

    }
}