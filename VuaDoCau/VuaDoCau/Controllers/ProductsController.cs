using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VuaDoCau.Data;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    public class ProductsController : Controller
    {
        private readonly VuaDoCauDbContext _db;

        public ProductsController(VuaDoCauDbContext db)
        {
            _db = db;
        }

        // GET: /Products?categoryId=&q=&sort=
        public IActionResult Index(int? categoryId, string q, string sort)
        {
            // 1) Query cơ sở
            var query = _db.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // 2) Lấy tạm ra list để lọc không dấu + không phân biệt hoa/thường
            var products = query.ToList();

            // 3) Tìm kiếm theo “chữ giống” (bỏ dấu + lower + gồm tất cả từ khóa)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var tokens = Tokenize(q);

                products = products.Where(p =>
                {
                    var haystack = Normalize(p.Name)
                                   + " " + Normalize(p.Category?.Name ?? string.Empty);
                    // Nếu có Mã/Code muốn tìm thêm thì cộng vào đây, ví dụ:
                    // haystack += " " + Normalize(p.Code ?? "");
                    return tokens.All(t => haystack.Contains(t));
                }).ToList();
            }

            // 4) Sắp xếp đơn giản (giữ nguyên nếu không truyền sort)
            products = sort?.ToLowerInvariant() switch
            {
                "new" or "date_desc" => products.OrderByDescending(p => p.Id).ToList(),
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                _ => products.OrderByDescending(p => p.Id).ToList()
            };

            return View(products);
        }

        // GET: /Products/Details/5
        public IActionResult Details(int id)
        {
            var p = _db.Products
                       .Include(x => x.Category)
                       .AsNoTracking()
                       .FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return View(p);
        }

        // ===== Helpers =====

        // Chuẩn hóa chuỗi: trim, lower, bỏ dấu, bỏ ký tự thừa, gộp khoảng trắng
        private static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Lower + Trim
            input = input.Trim().ToLowerInvariant();

            // Bỏ dấu tiếng Việt
            string formD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(capacity: formD.Length);
            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            // Bỏ ký tự không phải chữ/số (giữ khoảng trắng)
            var cleaned = new StringBuilder();
            foreach (var c in noDiacritics)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)) cleaned.Append(c);
            }

            // Gộp khoảng trắng
            var collapsed = System.Text.RegularExpressions.Regex.Replace(cleaned.ToString(), @"\s+", " ").Trim();
            return collapsed;
        }

        // Tách từ khóa thành list duy nhất (đã Normalize)
        private static List<string> Tokenize(string q)
        {
            var norm = Normalize(q);
            if (string.IsNullOrWhiteSpace(norm)) return new List<string>();
            return norm.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                       .Distinct()
                       .ToList();
        }
    }
}
