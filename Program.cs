using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Ecommerce;
using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Đăng ký dịch vụ
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<CartService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<BillService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<EmailService>();

// Đọc config MongoDB
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

// Đăng ký Identity dùng MongoDB
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
        mongoDbSettings.ConnectionString,
        mongoDbSettings.Name)
    .AddDefaultTokenProviders();

// Cấu hình Cookie Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// ✅ Bật Session (rất quan trọng)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

app.UseSession(); // ✅ Session phải trước Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}"
);

app.Run();
