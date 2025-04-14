using System;
using System.Collections.Generic;
using Kelimedya.Core.Enum;

namespace Kelimedya.Core.Models
{
    public class DashboardDto
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
        public int TotalRoles { get; set; }
        public int TotalProducts { get; set; }
        public int TotalReports { get; set; }
        public int TotalMessagesAll { get; set; }
        public List<OrderDto> LastOrders { get; set; } = new();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
    }
}