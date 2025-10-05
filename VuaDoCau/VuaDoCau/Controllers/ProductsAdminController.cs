using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    public class ProductsAdminController : Controller
    {
        private readonly VuaDoCauDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsAdminController(VuaDoCauDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /ProductsAdmin
        public async Task<IActionResult> Index()
        {
            var products = await _db.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: /ProductsAdmin/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            return View();
        }

        // POST: /ProductsAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? ImageFile)
        {
            // Fix lỗi "Category field is required"
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            // Nếu upload ảnh
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(_env.WebRootPath, "images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);
                model.ImageUrl = "/images/" + fileName;
            }
            else if (string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                model.ImageUrl = "/images/can-2.jpg"; // ảnh mặc định
            }

            _db.Products.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm sản phẩm mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /ProductsAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /ProductsAdmin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model, IFormFile? ImageFile)
        {
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            var product = await _db.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            // Cập nhật dữ liệu
            product.Sku = model.Sku;
            product.Name = model.Name;
            product.Slug = model.Slug;
            product.Summary = model.Summary;
            product.Price = model.Price;
            product.OldPrice = model.OldPrice;
            product.CategoryId = model.CategoryId;

            // Cập nhật ảnh nếu có upload mới
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(_env.WebRootPath, "images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);
                product.ImageUrl = "/images/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                product.ImageUrl = model.ImageUrl;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /ProductsAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /ProductsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa sản phẩm!";
            return RedirectToAction(nameof(Index));
        }
    }
}
