using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Services;

namespace VuaDoCau.Controllers;

public class CartController : Controller
{
    private readonly CartService _cart;
    private readonly VuaDoCauDbContext _db;
    public CartController(CartService cart, VuaDoCauDbContext db)
    { _cart = cart; _db = db; }

    public IActionResult Index()
    {
        var items = _cart.Get();
        ViewBag.Subtotal = items.Sum(x => x.LineTotal);
        ViewBag.Shipping = items.Any() ? 25000m : 0m;
        ViewBag.Total = (decimal)ViewBag.Subtotal + (decimal)ViewBag.Shipping;
        return View(items);
    }

    [HttpPost] // /cart/add/5?qty=1
    public async Task<IActionResult> Add(int id, int qty = 1)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        _cart.AddOrIncrease(new Models.CartItem
        {
            ProductId = p.Id,
            Name = p.Name,
            ImageUrl = p.ImageUrl ?? "/images/noimg.png",
            Price = p.Price,
            Quantity = Math.Max(1, qty)
        });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Update(int id, int qty)
    {
        _cart.Update(id, qty);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        _cart.Remove(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Clear()
    {
        _cart.Clear();
        return RedirectToAction(nameof(Index));
    }
}