using CV_siten.Data.Data;
using CV_siten.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- DATABAS & TJÄNSTER (Dependency Injection) ---

// Registrerar EF Core mot SQL Server.
// MigrationsAssembly anges explicit eftersom datalagret ligger i en separat assembly/mappstruktur.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Cv_siten.Data")));

// Konfigurerar Identity för användarhantering.
// RequireConfirmedAccount = false tillåter inloggning direkt utan e-postverifiering.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Anpassar inställningar för inloggnings-cookien.
// Här ställs säkerhet (HttpOnly) och livslängd in.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // Skyddar mot XSS-attacker
    options.Cookie.IsEssential = true;

    // Gör cookien till en sessions-cookie (försvinner när webbläsaren stängs)
    options.Cookie.MaxAge = null;
    options.Cookie.Expiration = null;

    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = false; // Tvingar utloggning efter 1h, oavsett aktivitet

    options.LoginPath = "/Account/Login"; // Vart användaren skickas om de inte är inloggade
});


// Registrerar MVC-ramverket (Controllers och Views) i DI-containern.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- HTTP REQUEST PIPELINE (Middleware) ---

// I produktionsmiljö hanterar vi fel och tvingar säkerhet (HSTS).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Tillåter åtkomst till filer i wwwroot (CSS, bilder, JS)

app.UseRouting();

// VIKTIGT: Ordningen är kritisk här.
// 1. Authentication: Systemet identifierar VEM användaren är.
// 2. Authorization: Systemet kontrollerar VAD användaren får göra.
app.UseAuthentication();
app.UseAuthorization();

// --- ROUTING ---
// Definierar standardmönstret för URL:er (Controller -> Action -> ev. ID)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();