// 1. UPPDATERADE USING-DIREKTIV
// Du behöver nu peka på de nya namespacen i ditt .Data-projekt.
// (Justera dessa om du valde andra namn på dina namespaces)
using CV_siten.Data.Data;   // För ApplicationDbContext
using CV_siten.Data.Models; // För ApplicationUser och andra modeller
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- DATABASE ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // 2. MIGRATIONS ASSEMBLY
        // Om du flyttade mappen 'Migrations' till .Data-projektet måste du berätta det här.
        // Detta gör att EF vet att det ska leta efter migrationstabellen i det andra projektet.
        b => b.MigrationsAssembly("CV_siten.Data")));

// --- IDENTITY ---
// Här används nu ApplicationUser från CV_siten.Data.Models
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Gör cookien till en session-cookie
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