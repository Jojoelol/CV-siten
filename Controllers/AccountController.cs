using CV_siten.Data;
using CV_siten.Models;
using CV_siten.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                    Beskrivning = model.Beskrivning ?? "", // Fixar förra felet
                    BildUrl = "", // LÄGG TILL DENNA RAD för att fixa nuvarande fel
                    AktivtKonto = true,
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
    }
}
