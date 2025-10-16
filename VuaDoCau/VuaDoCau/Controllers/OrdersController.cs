using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Text.Json;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    public class OrdersController : Controller
    {
        private const string CART_KEY = "CART";
        private readonly VuaDoCauDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;

        public OrdersController(VuaDoCauDbContext db, UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        // Form ràng buộc cho Checkout
        public class CheckoutForm
        {
            [Required, Display(Name = "Họ tên")]
            public string ReceiverName { get; set; } = string.Empty;

            [Required, Phone, Display(Name = "SĐT")]
            public string Phone { get; set; } = string.Empty;

            [EmailAddress]
            public string? Email { get; set; }

            [Required, Display(Name = "Địa chỉ")]
            public string Address { get; set; } = string.Empty;

            public string? Note { get; set; }
        }

        private class Line { public int ProductId { get; set; } public int Quantity { get; set; } }

        private List<Line> GetCartRaw()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            if (string.IsNullOrEmpty(json)) return new List<Line>();
            try { return JsonSerializer.Deserialize<List<Line>>(json) ?? new List<Line>(); }
            catch { return new List<Line>(); }
        }

        private List<dynamic> BuildItems()
        {
            var lines = GetCartRaw();
            if (lines.Count == 0) return new List<dynamic>();

            var ids = lines.Select(l => l.ProductId).Distinct().ToList();
            var prods = _db.Products.AsNoTracking()
                        .Where(p => ids.Contains(p.Id))
                        .ToDictionary(p => p.Id, p => p);

            var items = new List<dynamic>();
            foreach (var l in lines)
            {
                if (!prods.TryGetValue(l.ProductId, out var p)) continue;

                dynamic it = new ExpandoObject();
                it.ProductId = p.Id;
                it.Name = p.Name;
                it.ImageUrl = string.IsNullOrWhiteSpace(p.ImageUrl) ? "/images/no-image.png" : p.ImageUrl;
                it.UnitPrice = p.Price;
                it.Quantity = l.Quantity;
                it.Subtotal = p.Price * l.Quantity;
                items.Add(it);
            }
            return items;
        }

        private void FillTotals()
        {
            var items = BuildItems();
            decimal subtotal = items.Sum(it => (decimal)it.Subtotal);
            decimal shipping = items.Any() ? 25000m : 0m;
            ViewBag.Items = items;
            ViewBag.Subtotal = subtotal;
            ViewBag.Shipping = shipping;
            ViewBag.Total = subtotal + shipping;
        }

        // GET: /Orders/Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            var items = BuildItems();
            if (!items.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            FillTotals();

            var vm = new CheckoutForm();
            if (User.Identity?.IsAuthenticated == true)
            {
                var u = _userMgr.GetUserAsync(User).Result;
                if (u != null)
                {
                    vm.ReceiverName = u.FullName ?? "";
                    vm.Phone = u.PhoneNumber ?? "";
                    vm.Email = u.Email;
                    vm.Address = u.Address ?? "";
                }
            }
            return View(vm); // Views/Orders/Checkout.cshtml
        }

        // POST: /Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutForm form)
        {
            var items = BuildItems();
            if (!items.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                FillTotals();
                return View(form);
            }

            decimal subtotal = items.Sum(it => (decimal)it.Subtotal);
            decimal shipping = 25000m;

            var appUser = await _userMgr.GetUserAsync(User);

            var order = new Order
            {
                UserId = appUser?.Id ?? string.Empty,
                User = appUser, // đúng tên property trong model của bạn
                CreatedAt = DateTime.UtcNow,
                ReceiverName = form.ReceiverName,
                Phone = form.Phone,
                Email = form.Email ?? string.Empty,
                Address = form.Address,
                Note = form.Note,
                Subtotal = subtotal,
                ShippingFee = shipping,
                Status = "Pending"
            };

            foreach (dynamic it in items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = (int)it.ProductId,
                    Quantity = (int)it.Quantity,
                    UnitPrice = (decimal)it.UnitPrice
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove(CART_KEY);

            // tuỳ bạn: về trang thành công
            return RedirectToAction("Success", new { id = order.Id });
        }

        // Trang xác nhận thành công (tuỳ chọn)
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return RedirectToAction("Index", "Home");
            return View(order); // Tạo thêm Views/Orders/Success.cshtml nếu muốn
        }

        // Giữ alias cũ nếu ai đó gọi /Orders/Create
        [HttpGet] public IActionResult Create() => RedirectToAction(nameof(Checkout));
        [HttpPost, ValidateAntiForgeryToken]
        public Task<IActionResult> Create(CheckoutForm form) => Checkout(form);
    }
}
