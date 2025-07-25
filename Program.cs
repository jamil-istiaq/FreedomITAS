using FreedomITAS;
using FreedomITAS.Data;
using Microsoft.EntityFrameworkCore;
using FreedomITAS.Models;
using FreedomITAS.API_Settings;
using FreedomITAS.API_Serv;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
//using FreedomITAS.Services;


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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //options.Cookie.SameSite = SameSiteMode.None;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

//HaloPSA
builder.Services.Configure<HaloPSA>(builder.Configuration.GetSection("HaloPSA"));
builder.Services.AddTransient<HaloPSAService>();
builder.Services.AddHttpClient();


// Zomentum
builder.Services.Configure<ZomentumSettings>(builder.Configuration.GetSection("Zomentum"));
builder.Services.AddTransient<ZomentumService>();
builder.Services.AddHttpClient();

//Hudu
builder.Services.Configure<HuduSettings>(builder.Configuration.GetSection("Hudu"));
builder.Services.AddTransient<HuduService>();
builder.Services.AddHttpClient();


builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();
builder.Services.AddScoped<ClientPushService>();


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


using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var userEmail = "jamil@freedomit.com.au";
    var userPassword = "L#fz$6$E4UirjF";


    var user = await userManager.FindByEmailAsync(userEmail);
    if (user == null)
    {
        var newUser = new ApplicationUser

        {
            UserName = userEmail,
            Email = userEmail,
            EmailConfirmed = true,// optional but helps avoid issues
            FullName = "Jamil Istiaq",
            Role = "Admin"
        };

        var result = await userManager.CreateAsync(newUser, userPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error creating user: {error.Description}");
            }
        }
    }
}
app.Run();
