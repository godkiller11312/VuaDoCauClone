using System;
using System.Collections.Generic;

namespace VuaDoCau.Models
{
    public class ProfileViewModel
    {
        // Thông tin tài khoản
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";

        // Đơn hàng
        public List<OrderVM> Orders { get; set; } = new();
    }

    public class OrderVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusText { get; set; } = "";
        public decimal Total { get; set; }
        public List<OrderItemVM> Items { get; set; } = new();
    }

    public class OrderItemVM
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
