using FreedomITAS;
using FreedomITAS.Data;
using Microsoft.EntityFrameworkCore;
using FreedomITAS.Models;
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
});

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

app.UseAuthentication(); // <- required before UseAuthorization
app.UseAuthorization();

app.MapRazorPages();
app.Run();
