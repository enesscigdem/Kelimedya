using System.Text.Json.Serialization;

namespace Paragraph.Core.Entities
{
    public class ProductCourse
    {
        public int ProductId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}