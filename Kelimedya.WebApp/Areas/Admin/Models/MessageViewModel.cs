using System;
using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Areas.Admin.Models
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Gönderen e-posta bilgisi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string FromEmail { get; set; }
        
        [Required(ErrorMessage = "Konu alanı gereklidir.")]
        public string Subject { get; set; }
        
        [Required(ErrorMessage = "Mesaj içeriği gereklidir.")]
        public string Body { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}