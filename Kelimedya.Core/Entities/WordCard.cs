using Kelimedya.Core.BaseModels;

namespace Kelimedya.Core.Entities
{
    public class WordCard : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public int LessonId { get; set; }

        public string Word { get; set; } = null!;
        public string Synonym { get; set; } = null!;
        public string Definition { get; set; } = null!;
        public string? ExampleSentence { get; set; }
        public string? ImageUrl { get; set; }

    }
}