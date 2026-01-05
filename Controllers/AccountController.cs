using Microsoft.AspNetCore.Mvc;
using CV_siten.Models;
using CV_siten.Data;
using Microsoft.AspNetCore.Http;

namespace CV_siten.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
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
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Felaktig e-post eller lösenord.";
            return View(model);
        }

        public IActionResult Logout()
        {
            // Rensar inloggningsdatan
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // --- SKAPA KONTO ---
        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAccount(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kontrollera om e-posten redan finns
                var emailExists = _context.Persons.Any(p => p.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "E-postadressen är redan registrerad.");
                    return View(model);
                }

                // 2. Skapa ett nytt Person-objekt
                var nyPerson = new Person
                {
                    Fornamn = model.Fornamn,
                    Efternamn = model.Efternamn,
                    Email = model.Email,
                    Losenord = model.Losenord,
                    Telefonnummer = model.Telefonnummer,
                    Yrkestitel = model.Yrkestitel,
                    Beskrivning = model.Beskrivning,
                    Aktivtkonto = true,
                    BildUrl = ""
                };

                // 3. Spara i databasen
                _context.Persons.Add(nyPerson);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Ditt konto har skapats! Du kan nu logga in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }
    }
}