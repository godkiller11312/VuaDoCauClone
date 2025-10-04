namespace Vuadocau.Web.Models;


public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!; // ví dụ: can-shimano-xyz
    public string? Summary { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }


    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
}