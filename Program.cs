using FreedomITAS;
using FreedomITAS.Data;
using Microsoft.EntityFrameworkCore;
using FreedomITAS.Models;
using FreedomITAS.API_Settings;
using FreedomITAS.API_Serv;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddDataProtection();
builder.Services.AddScoped<RouteProtector>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {options.SignIn.RequireConfirmedAccount = false;})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Custom login path
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

//API HaloPSA
builder.Services.Configure<HaloPSA>(builder.Configuration.GetSection("HaloPSA"));
builder.Services.AddTransient<HaloPSAService>();
builder.Services.AddHttpClient();

// Zomentum
builder.Services.Configure<ZomentumSettings>(builder.Configuration.GetSection("Zomentum"));
builder.Services.AddTransient<ZomentumService>();
builder.Services.AddHttpClient();

//Hudu
builder.Services.Configure<HuduSettings>(builder.Configuration.GetSection("Hudo"));
builder.Services.AddTransient<HuduService>();
builder.Services.AddHttpClient();


builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();


builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Logout");
    options.Conventions.AllowAnonymousToPage("/Clients/ClientCreate");
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.SameAsRequest
});

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
