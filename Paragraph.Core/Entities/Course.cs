using Paragraph.Core.BaseModels;

namespace Paragraph.Core.Entities
{
    public class Course : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public int LessonCount { get; set; } = 40; 
        public int WordCount { get; set; } = 400;
        public string? ImageUrl { get; set; }
        public ICollection<ProductCourse> ProductCourses { get; set; }
            = new List<ProductCourse>();
    }
}