using Microsoft.AspNetCore.Identity;

namespace Paragraph.Core.IdentityEntities
{
    public class CustomUser : IdentityUser<int>
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? CreatedAt { get; set; }

        public int? CompanyId { get; set; }
        //public virtual Company Company { get; set; }
        
        public bool? AdminApproved { get; set; }
        public bool IsActive { get; set; } = true;
    }
}