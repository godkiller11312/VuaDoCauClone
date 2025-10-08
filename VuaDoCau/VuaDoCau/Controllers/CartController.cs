using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VuaDoCau.Data;

namespace VuaDoCau.Controllers
{
    public class CartController : Controller
    {
        private const string CART_KEY = "CART";
        private readonly VuaDoCauDbContext _db;

        public CartController(VuaDoCauDbContext db)
        {
            _db = db;
        }

        private class Line
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class ItemVM
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = "";
            public string? ImageUrl { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal => UnitPrice * Quantity;
        }

        public class CartVM
        {
            public List<ItemVM> Items { get; set; } = new();
            public decimal Shipping { get; set; } = 25000m; // phí ship mẫu
            public decimal Subtotal => Items.Sum(i => i.Subtotal);
            public decimal Total => Subtotal + (Items.Any() ? Shipping : 0);
        }

        // ===== Session helpers =====
        private List<Line> GetCartRaw()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            if (string.IsNullOrEmpty(json)) return new List<Line>();
            try { return JsonSerializer.Deserialize<List<Line>>(json) ?? new List<Line>(); }
            catch { return new List<Line>(); }
        }

        private void SaveCartRaw(List<Line> cart)
        {
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));
        }

        private CartVM BuildCartVM()
        {
            var lines = GetCartRaw();
            var ids = lines.Select(l => l.ProductId).Distinct().ToList();
            var prods = _db.Products
                           .AsNoTracking()
                           .Where(p => ids.Contains(p.Id))
                           .ToDictionary(p => p.Id, p => p);

            var vm = new CartVM();
            foreach (var l in lines)
            {
                if (!prods.TryGetValue(l.ProductId, out var p)) continue;
                vm.Items.Add(new ItemVM
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    ImageUrl = string.IsNullOrWhiteSpace(p.ImageUrl) ? "/images/no-image.png" : p.ImageUrl,
                    UnitPrice = p.Price,
                    Quantity = l.Quantity
                });
            }
            return vm;
        }

        // ===== Actions =====

        // Trang giỏ hàng
        public IActionResult Index()
        {
            return View(BuildCartVM());
        }

        // Add: hỗ trợ cả POST (đúng form) và GET (lỡ bấm mở link)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int quantity = 1)
        {
            return AddGet(productId, quantity);
        }

        [HttpGet]
        public IActionResult Add(int productId, int? quantity)
        {
            return AddGet(productId, quantity.GetValueOrDefault(1));
        }

        private IActionResult AddGet(int productId, int quantity)
        {
            if (productId <= 0) return RedirectToAction("Index", "Products");
            if (quantity < 1) quantity = 1;

            var cart = GetCartRaw();
            var line = cart.FirstOrDefault(x => x.ProductId == productId);
            if (line == null) cart.Add(new Line { ProductId = productId, Quantity = quantity });
            else line.Quantity += quantity;

            SaveCartRaw(cart);
            TempData["CartMessage"] = "Đã thêm vào giỏ!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCartRaw();
            var line = cart.FirstOrDefault(x => x.ProductId == productId);
            if (line != null)
            {
                line.Quantity = Math.Max(1, quantity);
                SaveCartRaw(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = GetCartRaw();
            cart.RemoveAll(x => x.ProductId == productId);
            SaveCartRaw(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            SaveCartRaw(new List<Line>());
            return RedirectToAction(nameof(Index));
        }
    }
}
