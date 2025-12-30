using System.Diagnostics;
using CV_siten.Models;
using CV_siten.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Krävs för sessioner

namespace CV_siten.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Hämtar den första personen för att visa på startsidan
            var profil = _context.Persons.FirstOrDefault();

            // Hämtar det senaste projektet baserat på Id
            ViewBag.SenasteProjekt = _context.Projekt
                                             .OrderByDescending(p => p.Id)
                                             .FirstOrDefault();

            return View(profil);
        }

        // --- INLOGGNING ---

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Letar efter matchande användare i SQL Server
            var user = _context.Persons.FirstOrDefault(p => p.Email == model.Email && p.Losenord == model.Losenord);

            if (user != null)
            {
                // Sparar användarens uppgifter i sessionen
                HttpContext.Session.SetInt32("AnvandareId", user.Id);
                HttpContext.Session.SetString("AnvandareNamn", user.Fornamn);
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Felaktig e-post eller lösenord.";
            return View(model);
        }

        public IActionResult Logout()
        {
            // Rensar inloggningsdatan
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // --- SKAPA KONTO ---

        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAccount(CreateAccountViewModel model)
        {
            // Kontrollera att alla Required-fält är ifyllda enligt din ViewModel
            if (ModelState.IsValid)
            {
                // 1. Kontrollera om e-posten redan finns för att undvika dubbletter
                var emailExists = _context.Persons.Any(p => p.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "E-postadressen är redan registrerad.");
                    return View(model);
                }

                // 2. Skapa ett nytt Person-objekt baserat på din modell
                var nyPerson = new Person
                {
                    Fornamn = model.Fornamn,
                    Efternamn = model.Efternamn,
                    Email = model.Email,
                    Losenord = model.Losenord,
                    Telefonnummer = model.Telefonnummer,
                    Yrkestitel = model.Yrkestitel,
                    Beskrivning = model.Beskrivning,
                    Aktivtkonto = true, // Kontot aktiveras direkt
                    BildUrl = ""
                };

                // 3. Spara den nya användaren i databasen
                _context.Persons.Add(nyPerson);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Ditt konto har skapats! Du kan nu logga in.";

                // Skickar användaren till inloggningen efter lyckad registrering
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}