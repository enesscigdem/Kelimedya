using System;
using System.Collections.Generic;

namespace Kelimedya.WebAPI.Models
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
        public List<OrderDto> LastOrders { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}