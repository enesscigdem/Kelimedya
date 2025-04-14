using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class ContactSchoolsModel
    {
        [Required(ErrorMessage = "Lütfen okul adını giriniz.")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "Lütfen ilgili kişiyi giriniz.")]
        public string ContactPerson { get; set; }

        // E-posta için property adı "FromEmail" olarak kullanılacak, formda name="FromEmail" olmalı.
        [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string FromEmail { get; set; }

        [Required(ErrorMessage = "Lütfen mesajınızı giriniz.")]
        public string Body { get; set; }

        public string Subject { get; set; }
    }
}