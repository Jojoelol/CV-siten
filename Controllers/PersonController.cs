using CV_siten.Data;
using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Controllers
{
    [Authorize] // Skyddar alla actions i denna controller
    public class PersonController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PersonController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Exempel-action: Visa min profil
        public async Task<IActionResult> MinProfil()
        {
            // 1. Hämta inloggad IdentityUser
            var identityUser = await _userManager.GetUserAsync(User);

            if (identityUser == null)
                return RedirectToAction("Login", "Account");

            // 2. Hämta Person kopplad till IdentityUser
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == identityUser.Id);

            if (person == null)
                return NotFound("Ingen Person-profil kopplad till detta konto.");

            // 3. Returnera view med Person-data
            return View(person);
        }
    }
}
