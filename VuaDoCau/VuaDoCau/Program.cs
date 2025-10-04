using Microsoft.EntityFrameworkCore;
using Vuadocau.Web.Data;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<VuadocauDbContext>(opt =>
opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();


app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");


// Auto-migrate & seed on startup (dev only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VuadocauDbContext>();
    db.Database.Migrate();
    DbInitializer.Seed(db);
}


app.Run();