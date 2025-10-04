using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers;

public class HomeController : Controller
{
    private readonly VuaDoCauDbContext _db;
    public HomeController(VuaDoCauDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Products.Include(p => p.Category)
                                .OrderByDescending(p => p.Id)
                                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || (p.Summary ?? "").Contains(q));

        var products = await query.Take(12).ToListAsync();
        ViewBag.Query = q;
        return View(products);
    }

    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}