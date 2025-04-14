using System;

namespace Kelimedya.WebApp.Areas.Admin.Models
{
    public class WidgetViewModel
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}