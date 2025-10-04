using Microsoft.EntityFrameworkCore;
using VuaDoCau.Data;
using VuaDoCau.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext - SQL Server
builder.Services.AddDbContext<VuaDoCauDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Session + helpers
builder.Services.AddHttpContextAccessor();        // để CartService dùng Session
builder.Services.AddScoped<CartService>();        // ĐĂNG KÝ CartService
builder.Services.AddSession(o =>                 // đã có thì giữ, chưa có thì thêm
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Auto migrate & seed (dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VuaDoCauDbContext>();
    db.Database.Migrate();
    DbInitializer.Seed(db);
}

app.Run();