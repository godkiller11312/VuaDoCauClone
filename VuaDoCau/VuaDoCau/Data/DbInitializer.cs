using VuaDoCau.Models;

namespace VuaDoCau.Data;

public static class DbInitializer
{
    public static void Seed(VuaDoCauDbContext db)
    {
        if (db.Categories.Any()) return;

        var cats = new[]
        {
            new Category { Slug = "can-cau", Name = "Cần câu" },
            new Category { Slug = "may-cau", Name = "Máy câu" },
            new Category { Slug = "day-moi-phu-kien", Name = "Dây / Mồi / Phụ kiện" }
        };
        db.Categories.AddRange(cats);
        db.SaveChanges();

        var products = new[]
        {
            new Product {
                Sku="CAN001", Name="Cần Shimano Bassterra 2.4m", Slug="can-shimano-bassterra-24",
                Summary="Carbon, 2 khúc, ném bờ", ImageUrl="/images/can-1.jpg", Price=1790000, OldPrice=1990000,
                CategoryId=cats[0].Id },
            new Product {
                Sku="MAY001", Name="Máy Daiwa Revros 2500", Slug="may-daiwa-revros-2500",
                Summary="Spinning, mượt, bền", ImageUrl="/images/may-1.jpg", Price=1490000,
                CategoryId=cats[1].Id },
            new Product {
                Sku="DAY001", Name="Dây PE X4 150m", Slug="day-pe-x4-150",
                Summary="Bền, chống mài mòn", ImageUrl="/images/day-1.jpg", Price=189000,
                CategoryId=cats[2].Id }
        };
        db.Products.AddRange(products);
        db.SaveChanges();
    }
}