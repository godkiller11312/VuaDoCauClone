﻿using System;
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

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
            if (user == null) return Challenge();

            var orders = _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .Select(o => new OrderVM
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status ?? "",
                    Total = o.Total,
                    Items = o.Items.Select(i => new OrderItemVM
                    {
                        ProductId = i.ProductId,                                // <-- THÊM
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = _db.Orders.Include(o => o.Items)
                                  .FirstOrDefault(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();

            var s = (order.Status ?? "").Trim();
            var isPending = s.Equals("Pending", StringComparison.OrdinalIgnoreCase)
                         || s.Equals("Chờ xác nhận", StringComparison.OrdinalIgnoreCase)
                         || s.Equals("Cho xac nhan", StringComparison.OrdinalIgnoreCase);

            if (!isPending)
            {
                TempData["Error"] = "Đơn đã được xác nhận/xử lý, không thể hủy.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Items?.Count > 0) _db.OrderItems.RemoveRange(order.Items);
            _db.Orders.Remove(order);
            _db.SaveChanges();

            TempData["Success"] = $"Đã hủy đơn hàng #{order.Id}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmReceived(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = _db.Orders.FirstOrDefault(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();

            var s = (order.Status ?? "").Trim();
            // Cho phép từ Shipping/Delivered/Paid ... => Completed
            if (!s.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Completed";
                _db.SaveChanges();
                TempData["Success"] = $"Đơn #{order.Id} đã hoàn tất.";
            }

            // Báo cho View biết hiển thị khối đánh giá cho đơn này
            TempData["RateOrderId"] = order.Id;

            return RedirectToAction(nameof(Index));
        }

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
