using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    [Route("reviews")]
    public class ReviewsController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        public ReviewsController(VuaDoCauDbContext db) => _db = db;

        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int stars, string? comment)
        {
            stars = Math.Clamp(stars, 1, 5);

            // kiểm tra tồn tại sản phẩm
            var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId);
            if (p == null) return NotFound();

            var review = new Review
            {
                ProductId = productId,
                Stars = stars,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
                UserId = User.Identity?.IsAuthenticated == true
                         ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                         : null
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("Details", "Products", new { id = productId });
        }
    }
}
