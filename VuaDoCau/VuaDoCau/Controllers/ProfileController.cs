using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(VuaDoCauDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Profile
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _userManager.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
            if (user == null) return Challenge();

            var orders = _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .Select(o => new OrderVM
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status ?? string.Empty,
                    Total = o.Total,
                    Items = o.Items.Select(i => new OrderItemVM
                    {
                        Quantity = i.Quantity,
                        ProductName = i.Product != null ? i.Product.Name : "(Sản phẩm)"
                    }).ToList()
                })
                .ToList();

            var vm = new ProfileViewModel
            {
                FullName = user.FullName ?? user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Address = user.Address ?? "",
                Orders = orders
            };

            return View(vm);
        }

        // POST: /Profile/Cancel  (User tự hủy đơn khi admin chưa xác nhận)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = _db.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            // Chỉ cho hủy khi chưa xác nhận (Pending/Chờ xác nhận)
            var status = (order.Status ?? string.Empty).Trim();
            var canCancel =
                status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Chờ xác nhận", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Cho xac nhan", StringComparison.OrdinalIgnoreCase);

            if (!canCancel)
            {
                TempData["Error"] = "Đơn đã được xác nhận/xử lý, không thể hủy.";
                return RedirectToAction(nameof(Index));
            }

            // Xóa đơn + dòng hàng (an toàn cả khi bật cascade)
            if (order.Items != null && order.Items.Count > 0)
                _db.OrderItems.RemoveRange(order.Items);

            _db.Orders.Remove(order);
            _db.SaveChanges();

            TempData["Success"] = $"Đã hủy đơn hàng #{order.Id}.";
            return RedirectToAction(nameof(Index));
        }

        // (Tuỳ chọn) Cập nhật hồ sơ cơ bản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(ProfileViewModel input)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return Challenge();

            user.FullName = input.FullName?.Trim();
            user.PhoneNumber = input.PhoneNumber?.Trim();
            user.Address = input.Address?.Trim();
            _db.SaveChanges();

            TempData["Success"] = "Đã cập nhật hồ sơ.";
            return RedirectToAction(nameof(Index));
        }
    }
}
