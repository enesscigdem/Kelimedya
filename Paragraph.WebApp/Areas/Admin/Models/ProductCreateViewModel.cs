using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Fiyat bilgisi gereklidir.")]
        public decimal Price { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        public List<int> SelectedCourseIds { get; set; }
            = new List<int>();

        public List<CourseViewModel> AvailableCourses { get; set; }
            = new List<CourseViewModel>();
    }
}