using System;
using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebApp.Areas.Admin.Models
{
    public class ReportViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Başlık alanı gereklidir.")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Açıklama alanı gereklidir.")]
        public string Description { get; set; }
        
        public DateTime? ReportDate { get; set; }
    }
}