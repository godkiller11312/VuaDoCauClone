using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy đơn hàng của user (đúng navigation: Items -> Product)
            var orders = await _db.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var vm = new ProfileViewModel
            {
                FullName = user.FullName ?? user.UserName ?? "",
                Email = user.Email ?? "",
                Phone = user.PhoneNumber ?? "",
                Address = user.Address ?? ""
            };

            foreach (var o in orders)
            {
                var ovm = new OrderVM
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    StatusText = o.Status ?? "—",
                    Total = o.Total // giả định Total = Subtotal + ShippingFee (đã có sẵn trong model)
                };

                foreach (var it in o.Items)
                {
                    ovm.Items.Add(new OrderItemVM
                    {
                        ProductName = it.Product?.Name ?? $"SP #{it.ProductId}",
                        Quantity = it.Quantity,
                        UnitPrice = it.UnitPrice
                    });
                }

                vm.Orders.Add(ovm);
            }

            return View(vm);
        }
    }
}
