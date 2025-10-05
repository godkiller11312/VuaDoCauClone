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
        private readonly UserManager<ApplicationUser> _userMgr;

        public ProfileController(VuaDoCauDbContext db, UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userMgr.GetUserAsync(User);
            if (user == null) return NotFound();

            var orders = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.User = user;
            return View(orders);
        }
    }
}
