using Microsoft.EntityFrameworkCore;
using VuaDoCau.Models;

namespace VuaDoCau.Data;

public class VuaDoCauDbContext : DbContext
{
    public VuaDoCauDbContext(DbContextOptions<VuaDoCauDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
    }
}