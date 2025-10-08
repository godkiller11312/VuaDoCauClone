﻿using Microsoft.AspNetCore.Mvc;
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
            var query = _db.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var products = query.ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var tokens = Tokenize(q);
                products = products.Where(p =>
                {
                    var haystack = Normalize(p.Name) + " " + Normalize(p.Category?.Name ?? "");
                    return tokens.All(t => haystack.Contains(t));
                }).ToList();
            }

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

            // Nếu view tên "Detail.cshtml" thì trả về theo tên này:
            return View("Detail", p);
        }

        // ===== Helpers (search normalize) =====
        private static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            input = input.Trim().ToLowerInvariant();

            string formD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            var cleaned = new StringBuilder();
            foreach (var c in noDiacritics)
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)) cleaned.Append(c);

            return System.Text.RegularExpressions.Regex.Replace(cleaned.ToString(), @"\s+", " ").Trim();
        }

        private static List<string> Tokenize(string q)
        {
            var norm = Normalize(q);
            if (string.IsNullOrWhiteSpace(norm)) return new List<string>();
            return norm.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        }
    }
}
