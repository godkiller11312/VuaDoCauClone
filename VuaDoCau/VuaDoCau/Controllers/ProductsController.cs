using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vuadocau.Web.Data;


namespace Vuadocau.Web.Controllers;


public class ProductsController : Controller
{
    private readonly VuadocauDbContext _db;
    public ProductsController(VuadocauDbContext db) => _db = db;


    // /products?cat=can-cau
    public async Task<IActionResult> Index(string? cat)
    {
        var categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
        ViewBag.Categories = categories;
        ViewBag.Cat = cat;


        var query = _db.Products.Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(cat))
            query = query.Where(p => p.Category.Slug == cat);


        var products = await query.OrderBy(p => p.Name).ToListAsync();
        return View(products);
    }


    // /products/detail/can-shimano-bassterra-24
    [Route("products/detail/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var p = await _db.Products.Include(x => x.Category)
        .FirstOrDefaultAsync(x => x.Slug == slug);
        if (p == null) return NotFound();
        return View(p);
    }
}