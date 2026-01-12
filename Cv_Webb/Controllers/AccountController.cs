using CV_siten.Data.Data;
using CV_siten.Data.Models;
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

        // --- REGISTRERING ---
        [HttpGet]
        public IActionResult CreateAccount() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Hantera bilduppladdning
            string? uniqueFileName = null;
            if (model.ImageFile is { Length: > 0 })
            {
                uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);
            }

            // 2. Skapa Identity-användaren
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 3. Skapa kopplad Person-profil
                var person = new Person
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    JobTitle = model.JobTitle,
                    Description = model.Description ?? "",
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    PostalCode = model.PostalCode,
                    City = model.City,
                    IdentityUserId = user.Id,
                    ImageUrl = uniqueFileName,
                    IsActive = true,
                    IsPrivate = false
                };

                _context.Persons.Add(person);
                await _context.SaveChangesAsync();

                // 4. Logga in och skicka vidare
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Hantera eventuella fel vid registrering
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // --- INLOGGNING ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            if (result.Succeeded) return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Felaktig e-post eller lösenord.");
            return View(model);
        }

        // --- UTLOGGNING ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // --- ÄNDRA LÖSENORD ---
        [Authorize, HttpGet]
        public IActionResult ChangePassword() => View();

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Lösenordet har ändrats!";
                return RedirectToAction("Edit", "Person");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}