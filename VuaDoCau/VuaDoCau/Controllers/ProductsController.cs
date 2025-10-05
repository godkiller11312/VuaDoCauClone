using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;

namespace VuaDoCau.Controllers
{
    public class ProductsController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        public ProductsController(VuaDoCauDbContext db) => _db = db;

        // /Products
        public async Task<IActionResult> Index(string? cat)
        {
            ViewBag.Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            ViewBag.Cat = cat;

            var query = _db.Products.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(cat))
                query = query.Where(p => p.Category.Slug == cat);

            var products = await query.OrderBy(p => p.Name).ToListAsync();
            return View(products);
        }

        // /Products/Category/2  (lọc theo CategoryId, dùng cho dropdown)
        [Route("Products/Category/{id:int}")]
        public async Task<IActionResult> Category(int id)
        {
            ViewBag.Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
            var products = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View("Index", products);
        }

        // /products/detail/{slug}
        [Route("products/detail/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            var p = await _db.Products.Include(x => x.Category)
                                      .FirstOrDefaultAsync(x => x.Slug == slug);
            if (p == null) return NotFound();
            return View(p);
        }
    }
}
