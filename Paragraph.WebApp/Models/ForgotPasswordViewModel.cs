using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; }
    }
}