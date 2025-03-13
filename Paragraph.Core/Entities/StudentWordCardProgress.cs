using Paragraph.Core.BaseModels;
using System;

namespace Paragraph.Core.Entities
{
    public class StudentWordCardProgress : IIntEntity, IActivateableEntity, IIsDeletedEntity, IAuditEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public int WordCardId { get; set; }
        public WordCard WordCard { get; set; }
        public int LessonId { get; set; }

        public bool IsLearned { get; set; }

        public int ViewCount { get; set; }
        // Aşağıdaki sütunlar kaldırıldı: CorrectAnswerCount, WrongAnswerCount, SuccessRate, IsMarkedForReview

        public DateTime FirstSeenDate { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public DateTime? LearnedDate { get; set; }

        // Yeni detaylı istatistik:
        public double ResponseTimeTotalSeconds { get; set; } // Toplam cevap süresi (saniye cinsinden)
        public double AverageResponseTimeSeconds 
        { 
            get => ViewCount > 0 ? ResponseTimeTotalSeconds / ViewCount : 0; 
            private set { } 
        }
    }
}
