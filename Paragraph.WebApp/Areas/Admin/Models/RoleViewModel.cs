using System.ComponentModel.DataAnnotations;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class RoleViewModel
    {
        [Required(ErrorMessage = "Rol ID gereklidir.")]
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Rol adı gereklidir.")]
        public string Name { get; set; }
    }
}