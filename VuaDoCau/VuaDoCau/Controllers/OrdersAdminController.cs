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
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToList();

            return View(orders);
        }

        // POST: /OrdersAdmin/MarkCompleted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkCompleted(int id)
        {
            var order = _db.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (!string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Completed";
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // (Tuỳ chọn) Chuyển sang đang giao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkShipping(int id)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            if (!string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Shipping";
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // (Tuỳ chọn) Huỷ đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            if (!string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Cancelled";
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
