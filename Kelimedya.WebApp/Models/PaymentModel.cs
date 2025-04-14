using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Models
{
    public class PaymentModel
    {
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public string CCV { get; set; }
        public CartDto Cart { get; set; } = new CartDto();
    }
}