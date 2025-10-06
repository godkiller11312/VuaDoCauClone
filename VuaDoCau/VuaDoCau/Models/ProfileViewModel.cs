using System;
using System.Collections.Generic;

namespace VuaDoCau.Models
{
    public class ProfileViewModel
    {
        // Thông tin user
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Danh sách đơn
        public List<OrderVM> Orders { get; set; } = new();
    }

    public class OrderVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }

        public List<OrderItemVM> Items { get; set; } = new();
    }

    public class OrderItemVM
    {
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
