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
            // 1. Kontrollera validering först
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Hantera bilduppladdningen (Spara filen på hårddisken)
            string? uniqueFileName = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Skapa ett unikt filnamn för att undvika krockar
                uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);

                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                // Skapa mappen om den inte finns
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
            }

            // 3. Skapa Identity-användaren (Inloggningsuppgifterna)
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 4. Skapa Person-profilen (Kopplad till Identity-användaren)
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
                    IdentityUserId = user.Id, // Kopplingen till inloggningskontot
                    ImageUrl = uniqueFileName, // Spara namnet på den uppladdade bilden
                    IsActive = true,
                    IsPrivate = false // Eller model.IsPrivate om du har den i din ViewModel
                };

                _context.Persons.Add(person);
                await _context.SaveChangesAsync();

                // 5. Logga in användaren direkt och skicka till startsidan
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // 6. Om registreringen misslyckades (t.ex. lösenordet för enkelt), lägg till felen i vyn
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

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

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

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
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // 1. HÅLL ANVÄNDAREN INLOGGAD (Viktigt!)
                await _signInManager.RefreshSignInAsync(user);

                // 2. SÄTT MEDDELANDET
                TempData["SuccessMessage"] = "Lösenordet har ändrats!";

                // 3. SKICKA TILLBAKA TILL REDIGERA PROFIL
                return RedirectToAction("Edit", "Person");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}