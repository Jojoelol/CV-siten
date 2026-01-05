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
        // GET: Account/Edit
        public IActionResult Edit()
        {
            // Hämta inloggad användares ID från sessionen
            var userId = HttpContext.Session.GetInt32("AnvandareId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Persons.Find(userId);
            if (user == null) return NotFound();

            // Fyll modellen med befintlig data från databasen
            var model = new EditAccountViewModel
            {
                Fornamn = user.Fornamn,
                Efternamn = user.Efternamn,
                Email = user.Email,
                Telefonnummer = user.Telefonnummer,
                Yrkestitel = user.Yrkestitel,
                Beskrivning = user.Beskrivning,
                Losenord = user.Losenord
            };

            return View("EditAccount", model);
        }

        [HttpPost]
        public IActionResult Edit(EditAccountViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("AnvandareId");
            if (userId == null) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var user = _context.Persons.Find(userId);
                if (user == null) return NotFound();

                // Uppdatera personens uppgifter
                user.Fornamn = model.Fornamn;
                user.Efternamn = model.Efternamn;
                user.Email = model.Email;
                user.Telefonnummer = model.Telefonnummer;
                user.Yrkestitel = model.Yrkestitel;
                user.Beskrivning = model.Beskrivning;

                _context.Update(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Profilen har uppdaterats.";
                return RedirectToAction("Index", "Home");
            }
            return View("EditAccount", model);
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