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
        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
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
                    Fornamn = model.Fornamn,
                    Efternamn = model.Efternamn,
                    Yrkestitel = model.Yrkestitel,
                    Beskrivning = model.Beskrivning ?? "", 
                    BildUrl = "", 
                    AktivtKonto = true,
                    Telefonnummer = model.Telefonnummer,
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
                isPersistent: false, 
                lockoutOnFailure: false
                );

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


        // --- VISA PROFIL ---

        [Authorize]
        public async Task<IActionResult> Profile(int? id, string searchString, string sortBy)
        {
            Person person;

            // 1. Hämta rätt person (antingen via ID i URL eller inloggad användare)
            if (id.HasValue)
            {
                person = await _context.Persons
                    .Include(p => p.PersonProjekt).ThenInclude(pp => pp.Projekt)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login");

                person = await _context.Persons
                    .Include(p => p.PersonProjekt).ThenInclude(pp => pp.Projekt)
                    .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            }

            if (person == null) return NotFound();

            // 2. Logik för sökning (filtrering av projektlistan)
            if (!string.IsNullOrEmpty(searchString))
            {
                person.PersonProjekt = person.PersonProjekt
                    .Where(pp => pp.Projekt.Projektnamn.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // 3. Logik för sortering
            person.PersonProjekt = sortBy switch
            {
                "status" => person.PersonProjekt.OrderBy(pp => pp.Projekt.Status).ToList(),
                "tid" => person.PersonProjekt.OrderByDescending(pp => pp.Projekt.Startdatum).ToList(),
                _ => person.PersonProjekt.OrderBy(pp => pp.Projekt.Projektnamn).ToList()
            };

            // Skicka med söksträngen så den stannar kvar i sökfältet efter omladdning
            ViewBag.CurrentSearch = searchString;

            // Denna rad tvingar programmet att hitta vyn i rätt mapp
            return View("~/Views/Account/Profile.cshtml", person);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null) return NotFound();

            var model = new EditAccountViewModel
            {
                Fornamn = person.Fornamn,
                Efternamn = person.Efternamn,
                Email = user.Email,
                Telefonnummer = person.Telefonnummer,
                Yrkestitel = person.Yrkestitel,
                Beskrivning = person.Beskrivning
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditAccount(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null) return NotFound();

            // Uppdatera IdentityUser
            user.Email = model.Email;
            user.UserName = model.Email;
            await _userManager.UpdateAsync(user);

            // Uppdatera Person
            person.Fornamn = model.Fornamn;
            person.Efternamn = model.Efternamn;
            person.Telefonnummer = model.Telefonnummer;
            person.Yrkestitel = model.Yrkestitel;
            person.Beskrivning = model.Beskrivning;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profilinformationen har uppdaterats.";
            return RedirectToAction("Profile");
        }

    }
}