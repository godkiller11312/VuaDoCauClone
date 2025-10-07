using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersAdminController : Controller
    {
        private readonly VuaDoCauDbContext _db;

        public OrdersAdminController(VuaDoCauDbContext db)
        {
            _db = db;
        }

        // GET: /OrdersAdmin
        public IActionResult Index()
        {
            var orders = _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToList();

            return View(orders);
        }

        // POST: /OrdersAdmin/MarkShipping  (Admin xác nhận giao)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkShipping(int id)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            // Chỉ cho chuyển từ trạng thái chờ xác nhận → đang giao
            var s = (order.Status ?? "").Trim();
            var isPending = s.Equals("Pending", StringComparison.OrdinalIgnoreCase)
                         || s.Equals("Chờ xác nhận", StringComparison.OrdinalIgnoreCase)
                         || s.Equals("Cho xac nhan", StringComparison.OrdinalIgnoreCase);

            if (isPending)
            {
                order.Status = "Shipping"; // Đang giao hàng
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // (tùy chọn) Hủy đơn (trước khi giao)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            var s = (order.Status ?? "").Trim();
            var canCancel = !s.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                            && !s.Equals("Shipping", StringComparison.OrdinalIgnoreCase);
            if (canCancel)
            {
                if (order.Items?.Count > 0) _db.OrderItems.RemoveRange(order.Items);
                _db.Orders.Remove(order);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
