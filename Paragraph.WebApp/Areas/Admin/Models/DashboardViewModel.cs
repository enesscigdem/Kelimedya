using System.Collections.Generic;

namespace Paragraph.WebApp.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalLessons { get; set; }
        public int TotalWordCards { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int EducationSales { get; set; }
        public int PendingPayments { get; set; }
        public int IncomingMessages { get; set; }
        public int TotalUsers { get; set; }
        // Ek metrikler:
        public int TotalRoles { get; set; }
        public int TotalProducts { get; set; }
        public int TotalReports { get; set; }
        public int TotalMessagesAll { get; set; }
        public List<OrderViewModel> LastOrders { get; set; }
    }
}