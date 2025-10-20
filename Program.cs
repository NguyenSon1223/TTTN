using Ecommerce.Services; // ✅ Import namespace của ProductService
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// Đăng ký MongoDB service (ProductService)
builder.Services.AddSingleton<ProductService>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Route mặc định (ProductController → Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}"
);

app.Run();
