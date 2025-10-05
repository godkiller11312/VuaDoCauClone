using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;

namespace VuaDoCau.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersAdminController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        public OrdersAdminController(VuaDoCauDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();
            if (order.Status == "Chờ xác nhận")
            {
                order.Status = "Đang giao hàng";
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đơn #{id} đã chuyển sang trạng thái 'Đang giao hàng'.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Done(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();
            if (order.Status == "Đang giao hàng")
            {
                order.Status = "Hoàn tất";
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đơn #{id} đã hoàn tất.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.Status = "Đã hủy";
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đơn #{id} đã bị hủy.";
            return RedirectToAction(nameof(Index));
        }
    }
}
