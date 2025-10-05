using VuaDoCau.Models;

namespace VuaDoCau.Data;

public static class DbInitializer
{
    public static void Seed(VuaDoCauDbContext db)
    {
        if (db.Categories.Any()) return; // tránh seed lặp khi đã có DB

        var cats = new[]
        {
            new Category { Slug = "can-cau", Name = "Cần câu" },
            new Category { Slug = "may-cau", Name = "Máy câu" },
            new Category { Slug = "day-moi-phu-kien", Name = "Dây / Mồi / Phụ kiện" }
        };
        db.Categories.AddRange(cats);
        db.SaveChanges();

        var products = new List<Product>
        {
            new() {Sku="CAN001",Name="Cần Shimano Bassterra 2.4m",Slug="can-shimano-bassterra-24",
                Summary="Carbon, 2 khúc, ném bờ",ImageUrl="/images/can-1.jpg",Price=1790000,OldPrice=1990000,CategoryId=cats[0].Id},
            new() {Sku="MAY001",Name="Máy Daiwa Revros 2500",Slug="may-daiwa-revros-2500",
                Summary="Spinning, bền, nhẹ",ImageUrl="/images/may-1.jpg",Price=1490000,CategoryId=cats[1].Id},
            new() {Sku="DAY001",Name="Dây PE X4 150m",Slug="day-pe-x4-150",
                Summary="Bền, chống mài mòn",ImageUrl="/images/day-1.jpg",Price=189000,CategoryId=cats[2].Id},

            // thêm nhiều sản phẩm mẫu
            new() {Sku="CAN002",Name="Cần Daiwa Crossfire 2.1m",Slug="can-daiwa-crossfire-21",
                Summary="Cần carbon 2 khúc, giá rẻ cho người mới",ImageUrl="/images/can-2.jpg",Price=890000,OldPrice=990000,CategoryId=cats[0].Id},
            new() {Sku="CAN003",Name="Cần Shimano Sojourn 2.4m",Slug="can-shimano-sojourn-24",
                Summary="Thiết kế cổ điển, độ nhạy cao",ImageUrl="/images/can-3.jpg",Price=1290000,CategoryId=cats[0].Id},
            new() {Sku="CAN004",Name="Cần Abu Garcia Veritas 2.7m",Slug="can-abu-veritas-27",
                Summary="Siêu nhẹ, dùng cho lure cá lóc",ImageUrl="/images/can-4.jpg",Price=2450000,CategoryId=cats[0].Id},
            new() {Sku="CAN005",Name="Cần Shimano Zodias 2024 2.1m",Slug="can-shimano-zodias-21",
                Summary="Cần cao cấp chuyên lure",ImageUrl="/images/can-5.jpg",Price=5200000,OldPrice=5500000,CategoryId=cats[0].Id},
            new() {Sku="CAN006",Name="Cần Pioneer Cobra 2.1m",Slug="can-pioneer-cobra-21",
                Summary="Phù hợp câu cá chép, cá trê",ImageUrl="/images/can-6.jpg",Price=790000,CategoryId=cats[0].Id},

            new() {Sku="MAY002",Name="Máy Shimano FX 2500",Slug="may-shimano-fx-2500",
                Summary="Tốc độ cuốn ổn định, drag 4kg",ImageUrl="/images/may-2.jpg",Price=890000,CategoryId=cats[1].Id},
            new() {Sku="MAY003",Name="Máy Daiwa Exceler LT 3000",Slug="may-daiwa-exceler-3000",
                Summary="Dòng LT siêu nhẹ, thân carbon",ImageUrl="/images/may-3.jpg",Price=1950000,OldPrice=2150000,CategoryId=cats[1].Id},
            new() {Sku="MAY004",Name="Máy Penn Battle III 4000",Slug="may-penn-battle-iii-4000",
                Summary="Cực khỏe, phù hợp câu biển",ImageUrl="/images/may-4.jpg",Price=3200000,CategoryId=cats[1].Id},
            new() {Sku="MAY005",Name="Máy Shimano Stradic FL 2500",Slug="may-shimano-stradic-fl-2500",
                Summary="Công nghệ Hagane Gear bền bỉ",ImageUrl="/images/may-5.jpg",Price=4250000,OldPrice=4550000,CategoryId=cats[1].Id},
            new() {Sku="MAY006",Name="Máy Okuma Ceymar 1000",Slug="may-okuma-ceymar-1000",
                Summary="Nhỏ gọn, 7 vòng bi, drag 3kg",ImageUrl="/images/may-6.jpg",Price=990000,CategoryId=cats[1].Id},

            new() {Sku="DAY002",Name="Dây câu fluorocarbon 100m",Slug="day-fluorocarbon-100m",
                Summary="Trong suốt, chịu lực êm",ImageUrl="/images/day-2.jpg",Price=159000,CategoryId=cats[2].Id},
            new() {Sku="DAY003",Name="Dây dù PE 8X YGK 150m",Slug="day-pe8x-ygk-150m",
                Summary="Dây 8 lõi cao cấp, dùng lure",ImageUrl="/images/day-3.jpg",Price=329000,OldPrice=379000,CategoryId=cats[2].Id},
            new() {Sku="DAY004",Name="Mồi giả cá nhái Jump Frog",Slug="moi-gia-nhai-jumpfrog",
                Summary="Dùng câu cá lóc, cá trê",ImageUrl="/images/moi-1.jpg",Price=59000,CategoryId=cats[2].Id},
            new() {Sku="DAY005",Name="Hộp đựng mồi lure đa năng",Slug="hop-dung-moi-da-nang",
                Summary="Chống nước, 12 ngăn",ImageUrl="/images/phukien-1.jpg",Price=129000,CategoryId=cats[2].Id},
            new() {Sku="DAY006",Name="Kìm gỡ cá inox 15cm",Slug="kim-go-ca-inox-15",
                Summary="Tháo lưỡi an toàn, chống gỉ",ImageUrl="/images/phukien-2.jpg",Price=99000,CategoryId=cats[2].Id},
            new() {Sku="DAY007",Name="Lưỡi câu Mustad số 8",Slug="luoi-cau-mustad-8",
                Summary="Lưỡi thép đen, cực bén",ImageUrl="/images/phukien-3.jpg",Price=49000,CategoryId=cats[2].Id},
            new() {Sku="DAY008",Name="Túi cần câu du lịch 1.2m",Slug="tui-can-cau-12m",
                Summary="Chống nước, 3 ngăn tiện dụng",ImageUrl="/images/phukien-4.jpg",Price=259000,OldPrice=289000,CategoryId=cats[2].Id},
        };

        db.Products.AddRange(products);
        db.SaveChanges();
    }
}