using VuaDoCau.Models;

namespace Vuadocau.Web.Models;


public class Category
{
    public int Id { get; set; }
    public string Slug { get; set; } = default!; // ví dụ: can-cau, may-cau
    public string Name { get; set; } = default!;
    public string? Description { get; set; }


    public ICollection<Product> Products { get; set; } = new List<Product>();
}