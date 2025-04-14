using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        
        public List<CourseViewModel> Courses { get; set; } = new List<CourseViewModel>();

        public List<ProductCourseViewModel> ProductCourses { get; set; } = new List<ProductCourseViewModel>();
        public DateTime CreatedAt { get; set; }

        public List<int> SelectedCourseIds { get; set; } = new List<int>();
        public List<CourseViewModel> AvailableCourses { get; set; } = new List<CourseViewModel>();

        public IFormFile? ImageFile { get; set; }
    }
    public class ProductCourseViewModel
    {
        public int ProductId { get; set; }
        public int CourseId { get; set; }
        public CourseViewModel? Course { get; set; }
    }
}