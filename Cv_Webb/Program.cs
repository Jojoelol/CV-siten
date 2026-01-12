
using CV_siten.Data.Data;   
using CV_siten.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- DATABASE ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Cv_siten.Data")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.Cookie.MaxAge = null;
    options.Cookie.Expiration = null;

    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = false;

    options.LoginPath = "/Account/Login";
});


// --- MVC ---
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- PIPELINE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Identity måste ligga här:
app.UseAuthentication();
app.UseAuthorization();




// --- ROUTING ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();