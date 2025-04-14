using System;
using Kelimedya.Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Areas.Admin.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Sipariş numarası gereklidir.")]
        public string OrderNumber { get; set; }
        
        [Required(ErrorMessage = "Sipariş tarihi gereklidir.")]
        public DateTime OrderDate { get; set; }
        
        [Required(ErrorMessage = "Müşteri adı gereklidir.")]
        public string CustomerName { get; set; }
        
        [Required(ErrorMessage = "Müşteri e-posta bilgisi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string CustomerEmail { get; set; }
        
        [Required(ErrorMessage = "Toplam tutar gereklidir.")]
        public decimal TotalAmount { get; set; }
        
        [Required(ErrorMessage = "Sipariş durumu seçilmelidir.")]
        public OrderStatus Status { get; set; }
    }
}