using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VuaDoCau.Models
{
    public class Product
    {
        public int Id { get; set; }

        // (tùy dự án của Chu đã có/không) – giữ nguyên các thuộc tính sẵn có
        public string? Sku { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }

        // >>> Thêm duy nhất dòng này để hiển thị mô tả ngắn:
        public string? Summary { get; set; }

        public string? ImageUrl { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        // FK
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
