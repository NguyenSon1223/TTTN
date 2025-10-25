using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Ecommerce;
using Ecommerce.Models;
using Ecommerce.Services; // ✅ Import namespace của ProductService
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();



builder.Services.AddSingleton<ProductService>();

var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        ).AddDefaultTokenProviders();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";      // Trang đăng nhập
        options.AccessDeniedPath = "/Account/Denied"; // Trang khi bị cấm
    });
//builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
//    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
//        builder.Configuration["MongoDbConfig:ConnectionString"],
//        builder.Configuration["MongoDbConfig:DatabaseName"])
//    .AddDefaultTokenProviders();

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
