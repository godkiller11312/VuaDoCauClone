using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;
using VuaDoCau.Data;

namespace VuaDoCau.Controllers
{
    public class OrdersController : Controller
    {
        private const string CART_KEY = "CART";
        private readonly VuaDoCauDbContext _db;

        public OrdersController(VuaDoCauDbContext db) => _db = db;

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
            var ids = lines.Select(l => l.ProductId).Distinct().ToList();
            var prods = _db.Products.Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id);

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

        private void FillTotalsToViewBag()
        {
            var items = BuildItems();
            decimal subtotal = 0m; foreach (dynamic it in items) subtotal += (decimal)it.Subtotal;
            decimal shipping = items.Any() ? 25000m : 0m;
            ViewBag.Items = items;
            ViewBag.Subtotal = subtotal;
            ViewBag.Shipping = shipping;
            ViewBag.Total = subtotal + shipping;
        }

        // ----- /Orders/Create  (GET)
        [HttpGet]
        public IActionResult Create()
        {
            var items = BuildItems();
            if (!items.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction("Index", "Cart");
            }
            FillTotalsToViewBag();
            return View(); // Views/Orders/Create.cshtml
        }

        // ----- /Orders/Create  (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string? fullName, string? phone, string? email, string? address, string? note)
        {
            var items = BuildItems();
            if (!items.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            // TODO: Map items -> Order/OrderItem của dự án nếu muốn lưu DB.
            HttpContext.Session.Remove(CART_KEY);
            TempData["CartMessage"] = "Đặt hàng thành công! Admin sẽ xác nhận giao hàng.";
            return RedirectToAction("Index", "Cart");
        }

        // ----- Alias để ai trỏ /Orders/Checkout vẫn dùng được
        [HttpGet]
        public IActionResult Checkout() => Create();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string? fullName, string? phone, string? email, string? address, string? note)
            => Create(fullName, phone, email, address, note);
    }
}
