namespace Kelimedya.WebAPI.Models
{
    public class TranscriptDto
    {
        public string HtmlContent { get; set; }
        public string PdfDownloadUrl { get; set; }
        public string StudentName { get; set; }
    }
    public class DetailedReportDto
    {
        public string HtmlContent { get; set; }
        public string PdfDownloadUrl { get; set; }
        public string StudentName { get; set; }
    }
}