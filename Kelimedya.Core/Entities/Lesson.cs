using Kelimedya.Core.BaseModels;
using System.Collections.Generic;

namespace Kelimedya.Core.Entities
{
    public class Lesson : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int SequenceNo { get; set; }
        public string? ImageUrl { get; set; }
        
        public virtual ICollection<StudentLessonProgress> StudentLessonProgresses { get; set; }
            = new List<StudentLessonProgress>();

        public virtual ICollection<WordCard> WordCards { get; set; }
            = new List<WordCard>();
    }
}