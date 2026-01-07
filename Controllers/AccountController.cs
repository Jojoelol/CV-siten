using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels.Account; // Se till att detta namespace stämmer
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

        // --- REGISTRERING ---
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

            // 1. Ändrat model.Losenord -> model.Password
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var person = new Person
                {
                    // 2. Ändrat till engelska properties från model
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    JobTitle = model.JobTitle,
                    Description = model.Description ?? "",
                    ImageUrl = "",
                    CvUrl = "",
                    IsActive = true,
                    PhoneNumber = model.PhoneNumber,
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

        // --- ÄNDRA LÖSENORD ---
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

        // --- INLOGGNING ---
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

            // 3. Ändrat model.Losenord -> model.Password
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false
                );

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Felaktig e-post eller lösenord.");
            return View(model);
        }

        // --- LOGOUT ---
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

            if (id.HasValue)
            {
                person = await _context.Persons
                    .Include(p => p.PersonProjects)
                    .ThenInclude(pp => pp.Project)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login");

                person = await _context.Persons
                    .Include(p => p.PersonProjects)
                    .ThenInclude(pp => pp.Project)
                    .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            }

            if (person == null) return NotFound();

            if (!string.IsNullOrEmpty(searchString))
            {
                person.PersonProjects = person.PersonProjects
                    .Where(pp => pp.Project.ProjectName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            person.PersonProjects = sortBy switch
            {
                "status" => person.PersonProjects.OrderBy(pp => pp.Project.Status).ToList(),
                "tid" => person.PersonProjects.OrderByDescending(pp => pp.Project.StartDate).ToList(),
                _ => person.PersonProjects.OrderBy(pp => pp.Project.ProjectName).ToList()
            };

            ViewBag.CurrentSearch = searchString;
            return View("~/Views/Account/Profile.cshtml", person);
        }

        // --- REDIGERA PROFIL ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null) return NotFound();

            // 4. Mappa till engelska properties i ViewModel
            var model = new EditAccountViewModel
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = user.Email,
                PhoneNumber = person.PhoneNumber,
                JobTitle = person.JobTitle,
                Description = person.Description
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

            user.Email = model.Email;
            user.UserName = model.Email;
            await _userManager.UpdateAsync(user);

            // 5. Mappa från engelska properties i ViewModel till Person-modell
            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            person.PhoneNumber = model.PhoneNumber;
            person.JobTitle = model.JobTitle;
            person.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profilinformationen har uppdaterats.";
            return RedirectToAction("Profile");
        }
    }
}