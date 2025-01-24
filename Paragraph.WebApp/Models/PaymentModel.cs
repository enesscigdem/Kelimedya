using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Models
{
    public class PaymentModel
    {
        [Required]
        [Display(Name = "Kart Sahibi")]
        public string CardHolderName { get; set; }

        [Required]
        [CreditCard]
        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; }

        [Required]
        [Display(Name = "Son Kullanma Tarihi (AA/YY)")]
        public string ExpirationDate { get; set; }

        [Required]
        [Range(100, 999, ErrorMessage = "Geçerli bir CVV numarası giriniz.")]
        [Display(Name = "CVV")]
        public int CVV { get; set; }

        [Required]
        [Display(Name = "Tutar")]
        public decimal Amount { get; set; }
    }
}