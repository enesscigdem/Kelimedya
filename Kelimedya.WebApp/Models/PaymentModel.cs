// Kelimedya.WebApp.Models.PaymentModel.cs

using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class PaymentModel
    {
        // Card fields
        [Required, CreditCard]
        public string CardNumber { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        [Required, RegularExpression(@"^\d{2}/\d{2}$", ErrorMessage = "AA/YY formatında giriniz.")]
        public string ExpiryDate { get; set; }

        [Required, StringLength(4, MinimumLength = 3)]
        public string CCV { get; set; }

        // Cart & coupon
        public CartDto Cart { get; set; } = new CartDto();

        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }

        // Computed grand total = Cart.TotalPrice – DiscountAmount
        public decimal GrandTotal => Cart.Total - DiscountAmount;
    }
}