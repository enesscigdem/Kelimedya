using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kelimedya.WebAPI.Models
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        public List<int>? CourseIds { get; set; }
    }
}