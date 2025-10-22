using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Ecommerce;
using Ecommerce.Models;
using Ecommerce.Services; // ✅ Import namespace của ProductService
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// Đăng ký MongoDB service (ProductService)

builder.Services.AddSingleton<ProductService>();
//builder.Services.AddSingleton<UserService>();

var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        );

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();

// Route mặc định (ProductController → Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}"
);

app.Run();
