namespace Kelimedya.WebAPI.Models
{
    public class AddToCartDto
    {
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}