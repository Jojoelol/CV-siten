using Microsoft.AspNetCore.Mvc;
using CV_siten.Models;
using CV_siten.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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
            var user = _context.Persons.FirstOrDefault(p => p.Email == model.Email && p.Losenord == model.Losenord);

            if (user != null)
            {
                // Vi använder "AnvandareId" konsekvent i hela controllern
                HttpContext.Session.SetInt32("AnvandareId", user.Id);
                HttpContext.Session.SetString("AnvandareNamn", user.Fornamn);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Felaktig e-post eller lösenord.";
            return View(model);
        }

        // --- REDIGERA PROFIL ---
        public IActionResult Edit()
        {
            var userId = HttpContext.Session.GetInt32("AnvandareId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Persons.Find(userId);
            if (user == null) return NotFound();

            var model = new EditAccountViewModel
            {
                Fornamn = user.Fornamn,
                Efternamn = user.Efternamn,
                Email = user.Email,
                Telefonnummer = user.Telefonnummer,
                Yrkestitel = user.Yrkestitel,
                Beskrivning = user.Beskrivning,
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

        // --- ÄNDRA LÖSENORD ---
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("AnvandareId");
            if (userId == null) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ÄNDRAT: Hämtar nu från "AnvandareId" för att matcha Login-metoden
            var userId = HttpContext.Session.GetInt32("AnvandareId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Persons.FindAsync(userId);
            if (user == null) return NotFound();

            // Kontrollera att det nuvarande lösenordet stämmer
            if (user.Losenord != model.OldPassword)
            {
                ModelState.AddModelError("OldPassword", "Det nuvarande lösenordet är felaktigt.");
                return View(model);
            }

            // Uppdatera lösenordet
            user.Losenord = model.NewPassword;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ditt lösenord har ändrats!";

            // ÄNDRAT: Redirect till "Edit" eftersom det är så din metod heter
            return RedirectToAction("Edit");
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
                var emailExists = _context.Persons.Any(p => p.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "E-postadressen är redan registrerad.");
                    return View(model);
                }

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

                _context.Persons.Add(nyPerson);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Ditt konto har skapats! Du kan nu logga in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}