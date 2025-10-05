using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Models;
using VuaDoCau.Services;

namespace VuaDoCau.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        private readonly CartService _cart;

        public OrdersController(VuaDoCauDbContext db, CartService cart)
        { _db = db; _cart = cart; }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _db.Users.OfType<ApplicationUser>()
                .FirstAsync(u => u.UserName == User.Identity!.Name);

            var vm = new CheckoutVm
            {
                ReceiverName = user.FullName ?? "",
                Phone = user.PhoneNumber ?? "",
                Email = user.Email ?? "",
                Address = user.Address ?? "",
                Items = _cart.Get()
            };
            vm.Subtotal = vm.Items.Sum(i => i.LineTotal);
            vm.Shipping = vm.Items.Any() ? 25000m : 0m;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutVm vm)
        {
            vm.Items = _cart.Get();
            vm.Subtotal = vm.Items.Sum(i => i.LineTotal);
            vm.Shipping = vm.Items.Any() ? 25000m : 0m;
            if (!vm.Items.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng trống.");
                return View(vm);
            }

            var user = await _db.Users.OfType<ApplicationUser>()
                .FirstAsync(u => u.UserName == User.Identity!.Name);

            var order = new Order
            {
                UserId = user.Id,
                ReceiverName = vm.ReceiverName,
                Phone = vm.Phone,
                Email = vm.Email,
                Address = vm.Address,
                Note = vm.Note,
                Subtotal = vm.Subtotal,
                ShippingFee = vm.Shipping,
                Status = "Chờ xác nhận"   // <<< MẶC ĐỊNH
            };

            foreach (var it in vm.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = it.ProductId,
                    Quantity = it.Quantity,
                    UnitPrice = it.Price
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            _cart.Clear();

            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        public async Task<IActionResult> Success(int id)
        {
            var o = await _db.Orders.Include(x => x.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            return View(o);
        }
    }

    public class CheckoutVm
    {
        public string ReceiverName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string? Note { get; set; }

        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total => Subtotal + Shipping;
    }
}
