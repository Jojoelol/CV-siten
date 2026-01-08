using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;

namespace CV_siten.Controllers
{
    [Authorize]
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PersonController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- VISA PROFIL (G-krav 1, 6 & VG-krav 1) ---
        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? id, string searchString, string sortBy)
        {
            var user = await _userManager.GetUserAsync(User);
            var loggedInPerson = user != null
                ? await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id)
                : null;

            if (!id.HasValue && loggedInPerson == null) return RedirectToAction("Login", "Account");
            int targetId = id ?? loggedInPerson.Id;

            var person = await _context.Persons
                .Include(p => p.IdentityUser)
                .Include(p => p.PersonProjects).ThenInclude(pp => pp.Project)
                .FirstOrDefaultAsync(p => p.Id == targetId);

            if (person == null) return NotFound();

            // Säkerställ att vi inte räknar besök vid t.ex. bildanrop (Fix för dubbelräkning)
            if (Request.Headers["Accept"].ToString().Contains("text/html"))
            {
                person.ViewCount++; // VG-krav #1
                await _context.SaveChangesAsync();
            }

            // Hantera privat profil (G-krav 7)
            if (person.IsPrivate && user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.IsOwner = (loggedInPerson != null && targetId == loggedInPerson.Id);

            // Sökning och sortering i projekt
            if (!string.IsNullOrEmpty(searchString))
            {
                person.PersonProjects = person.PersonProjects
                    .Where(pp => pp.Project.ProjectName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            person.PersonProjects = sortBy switch
            {
                "status" => person.PersonProjects.OrderBy(pp => pp.Project.Status).ToList(),
                "tid" => person.PersonProjects.OrderByDescending(pp => pp.Project.StartDate).ToList(),
                _ => person.PersonProjects.OrderBy(pp => pp.Project.ProjectName).ToList()
            };

            ViewBag.CurrentSearch = searchString;
            return View(person);
        }

        // --- REDIGERA PROFIL (GET) ---
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null) return NotFound();

            var model = new EditAccountViewModel
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = user.Email,
                PhoneNumber = person.PhoneNumber,
                JobTitle = person.JobTitle,
                Description = person.Description,
                ImageUrl = person.ImageUrl // Viktigt för att visa bilden i Edit-vyn
            };
            return View(model);
        }

        // --- REDIGERA PROFIL (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null) return NotFound();

            // 1. Hantera ny bilduppladdning
            if (model.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "ProfilePicture");

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Sparar "ProfilePicture/filnamn.jpg"
                person.ImageUrl = "ProfilePicture/" + fileName;
            }
            // Om ingen ny fil valts, behåll värdet som fanns i person.ImageUrl sedan tidigare.
            // Vi rör inte person.ImageUrl här om model.ImageFile är null.

            // 2. Uppdatera övriga fält
            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            person.PhoneNumber = model.PhoneNumber;
            person.JobTitle = model.JobTitle;
            person.Description = model.Description;

            user.Email = model.Email;
            user.UserName = model.Email;

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profilen har uppdaterats!";
            return RedirectToAction(nameof(Profile), new { id = person.Id });
        }

        // --- EXPORTERA TILL XML (VG-krav 7) ---
        [HttpGet]
        public async Task<IActionResult> ExportToXml()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .Include(p => p.PersonProjects).ThenInclude(pp => pp.Project)
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null) return NotFound();

            // Skapa en export-struktur för att undvika problem med tunga objekt
            var export = new
            {
                Namn = $"{person.FirstName} {person.LastName}",
                Titel = person.JobTitle,
                Beskrivning = person.Description,
                Kompetenser = person.Skills,
                Projekt = person.PersonProjects.Select(pp => new { pp.Project.ProjectName, pp.Project.Description }).ToList()
            };

            var serializer = new XmlSerializer(export.GetType());
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, export);
                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), "application/xml", $"CV_{person.LastName}.xml");
            }
        }

        // --- AKTIVERA / INAKTIVERA (VG-krav 3) ---
        [HttpPost]
        public async Task<IActionResult> Inactivate(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person != null) { person.IsActive = false; await _context.SaveChangesAsync(); }
            return RedirectToAction("Profile", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person != null) { person.IsActive = true; await _context.SaveChangesAsync(); }
            return RedirectToAction("Profile", new { id = id });
        }

        // --- REDIGERA CV-FÄLT (G-krav 4c) ---
        [HttpPost]
        public async Task<IActionResult> UpdateProfileField(int id, string fieldName, string fieldValue)
        {
            var person = await _context.Persons.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (person == null || person.IdentityUserId != user.Id) return Forbid();

            switch (fieldName)
            {
                case "Skills": person.Skills = fieldValue; break;
                case "Education": person.Education = fieldValue; break;
                case "Experience": person.Experience = fieldValue; break;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", new { id = id });
        }

        // --- CV-FILSHANTERING ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCv(IFormFile cvFile)
        {
            if (cvFile == null || Path.GetExtension(cvFile.FileName).ToLower() != ".pdf") return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null)
            {
                string fileName = Guid.NewGuid().ToString() + ".pdf";
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs", fileName);

                using (var stream = new FileStream(path, FileMode.Create)) { await cvFile.CopyToAsync(stream); }
                person.CvUrl = "/uploads/cvs/" + fileName;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Profile));
        }
    }
}