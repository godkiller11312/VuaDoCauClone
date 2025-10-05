using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VuaDoCau.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? Sku { get; set; }

        [MaxLength(120)]
        public string? Slug { get; set; }

        [MaxLength(200)]
        public string? Summary { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        // 🔴 KHÔNG đánh [Required] cho navigation
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }   // cho phép null ở đây

        [MaxLength(300)]
        public string? ImageUrl { get; set; }
    }
}
