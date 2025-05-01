using System;
using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Areas.Admin.Models
{
    public class CreateCouponDto
    {
        [Required(ErrorMessage = "Kupon kodu zorunludur.")]
        [StringLength(50, ErrorMessage = "Kupon kodu en fazla {1} karakter olabilir.")]
        [Display(Name = "Kupon Kodu")]
        public string Code { get; set; } = null!;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "İndirim türü seçimi zorunludur.")]
        [Range(0, 1, ErrorMessage = "İndirim türü geçersiz.")]
        [Display(Name = "İndirim Türü")]
        public byte DiscountType { get; set; }

        [Required(ErrorMessage = "İndirim değeri zorunludur.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Geçerli bir sayı giriniz (ör: 100 veya 12.50).")]
        [Display(Name = "İndirim Değeri")]
        [DataType(DataType.Text)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.##}")]
        public decimal? DiscountValue { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ValidFrom { get; set; }
        public DateTime? CreatedAt { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ValidTo { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCouponDto : CreateCouponDto
    {
        [Required]
        public int Id { get; set; }
    }
}