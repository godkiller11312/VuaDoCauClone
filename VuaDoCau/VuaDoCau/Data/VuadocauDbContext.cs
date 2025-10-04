using Microsoft.EntityFrameworkCore;
using Vuadocau.Web.Models;


namespace Vuadocau.Web.Data;


public class VuadocauDbContext : DbContext
{
    public VuadocauDbContext(DbContextOptions<VuadocauDbContext> options) : base(options) { }


    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
    }
}