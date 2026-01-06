using CV_siten.Data;
using CV_siten.Models;
using CV_siten.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        //REGISTRERING
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Losenord);

            if (result.Succeeded)
            {
                var person = new Person
                {
                    Fornamn = "",
                    Efternamn = "",
                    AktivtKonto  = true,
                    IdentityUserId = user.Id
                };

                _context.Persons.Add(person);
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        //CHANGEPASSWORD
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(
                identityUser,
                model.OldPassword,
                model.NewPassword
            );

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Lösenordet har ändrats.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }



        //INLOGGNING
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Losenord,
                false,
                false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Felaktig e-post eller lösenord.");
            return View(model);
        }

        //LOGOUT
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        // --- VISA PROFIL ---
        public async Task<IActionResult> Profile(int? id)
        {
            // 1. Bestäm vilket ID som ska visas. 
            // Om id saknas i URL:en, kolla om det finns en inloggad användare i sessionen.
            int? targetUserId = id ?? HttpContext.Session.GetInt32("AnvandareId");

            if (targetUserId == null)
            {
                // Om inget ID skickats med och ingen är inloggad, skicka till Login
                return RedirectToAction("Login");
            }

            // 2. Hämta personen från databasen. 
            // Vi använder .Include för att även hämta kopplingen till projekt 
            // och sedan .ThenInclude för att hämta själva projekt-datan.
            var person = await _context.Persons
                .Include(p => p.PersonProjekt)
                    .ThenInclude(pp => pp.Projekt)
                .FirstOrDefaultAsync(p => p.Id == targetUserId);

            if (person == null)
            {
                return NotFound();
            }

            // 3. Skicka person-objektet till vyn
            return View(person);
        }

        // Lägg till i AccountController.cs
        public async Task<IActionResult> Profile(int? id, string searchString, string sortBy)
        {
            // 1. Identifiera vilken person som ska visas
            int? targetUserId = id ?? HttpContext.Session.GetInt32("AnvandareId");
            if (targetUserId == null) return RedirectToAction("Login");

            // 2. Hämta personen och dess projekt från databasen
            var person = await _context.Persons
                .Include(p => p.PersonProjekt)
                    .ThenInclude(pp => pp.Projekt)
                .FirstOrDefaultAsync(p => p.Id == targetUserId);

            if (person == null) return NotFound();

            // 3. Logik för sökning i projektlistan
            if (!string.IsNullOrEmpty(searchString))
            {
                // Vi filtrerar listan i minnet (eftersom vi redan hämtat personen)
                person.PersonProjekt = person.PersonProjekt
                    .Where(pp => pp.Projekt.Projektnamn.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // 4. Logik för sortering
            person.PersonProjekt = sortBy switch
            {
                "status" => person.PersonProjekt.OrderBy(pp => pp.Projekt.Status).ToList(),
                "tid" => person.PersonProjekt.OrderByDescending(pp => pp.Projekt.Startdatum).ToList(),
                _ => person.PersonProjekt.OrderBy(pp => pp.Projekt.Projektnamn).ToList()
            };

            // Skicka med söksträngen tillbaka till vyn så den syns i sökfältet
            ViewBag.CurrentSearch = searchString;

            return View(person);
        }




    }
}