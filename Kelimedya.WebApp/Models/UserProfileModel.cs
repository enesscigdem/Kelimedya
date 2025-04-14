using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class UserProfileModel
    {
        [Required]
        [Display(Name = "Adınız")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyadınız")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "E-posta Adresiniz")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Telefon Numaranız")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Adresiniz")]
        public string Address { get; set; }

    }
}