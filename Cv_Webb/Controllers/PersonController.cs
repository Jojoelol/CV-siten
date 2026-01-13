using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels.Account;
using CV_siten.Services; // <-- Viktigt: Använder din nya service
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using System.Text;

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

        // --- VISA PROFIL ---
        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? id, string searchString, string sortBy)
        {
            var user = await _userManager.GetUserAsync(User);
            var loggedInPerson = user != null
                ? await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id)
                : null;

            // Om inget ID anges, visa inloggads profil. Annars redirect till login.
            if (!id.HasValue && loggedInPerson == null) return RedirectToAction("Login", "Account");
            int targetId = id ?? loggedInPerson.Id;

            var person = await _context.Persons
                .Include(p => p.IdentityUser)
                .Include(p => p.PersonProjects)
                    .ThenInclude(pp => pp.Project)
                        .ThenInclude(proj => proj.PersonProjects)
                .FirstOrDefaultAsync(p => p.Id == targetId);

            if (person == null) return NotFound();

            // Säkerhet: Kontrollera privat profil
            if (person.IsPrivate && user == null) return RedirectToAction("Login", "Account");

            // ViewCount
            if (Request.Headers["Accept"].ToString().Contains("text/html"))
            {
                person.ViewCount++;
                await _context.SaveChangesAsync();
            }

            // Metadata
            ViewBag.IsOwner = (loggedInPerson != null && targetId == loggedInPerson.Id);
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchString;

            // Sök/Sortering av Projekt
            if (person.PersonProjects != null)
            {
                var projects = person.PersonProjects.AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    projects = projects.Where(pp =>
                        pp.Project.ProjectName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        (pp.Role != null && pp.Role.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    );
                }

                projects = sortBy switch
                {
                    "name_asc" => projects.OrderBy(pp => pp.Project.ProjectName),
                    "name_desc" => projects.OrderByDescending(pp => pp.Project.ProjectName),
                    "status_planerat" => projects.OrderByDescending(pp => pp.Project.Status == "Planerat").ThenBy(pp => pp.Project.ProjectName),
                    "status_aktivt" => projects.OrderByDescending(pp => pp.Project.Status == "Aktivt").ThenBy(pp => pp.Project.ProjectName),
                    "status_avslutat" => projects.OrderByDescending(pp => pp.Project.Status == "Avslutat").ThenBy(pp => pp.Project.ProjectName),
                    "members_desc" => projects.OrderByDescending(pp => pp.Project.PersonProjects.Count),
                    "tid" => projects.OrderByDescending(pp => pp.Project.StartDate),
                    _ => projects.OrderBy(pp => pp.Project.ProjectName)
                };

                person.PersonProjects = projects.ToList();
            }

            // --- LIKNANDE PERSONER (Matchning via Service) ---
            var others = await _context.Persons
                .Where(p => p.Id != targetId && p.IsActive && !p.IsPrivate)
                .ToListAsync();

            var matches = others.Select(p =>
            {
                // HÄR ANROPAS DIN NYA SERVICE
                var score = MatchingService.CalculateMatchScore(person, p);
                var percent = Math.Min(score * 10, 100);
                return new { Person = p, Score = score, MatchPercent = percent };
            })
            .Where(x => x.MatchPercent >= 50) // Justera gränsen vid behov
            .OrderByDescending(x => x.MatchPercent)
            .Take(4)
            .ToList();

            ViewBag.SimilarPersons = matches;

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
                ImageUrl = person.ImageUrl,
                Address = person.Address,
                PostalCode = person.PostalCode,
                City = person.City,
                IsPrivate = person.IsPrivate
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

            // Använd hjälpmetoden för bild
            if (model.ImageFile != null)
            {
                // Spara i images/ProfilePicture
                string fileName = await SaveFileAsync(model.ImageFile, Path.Combine("images", "ProfilePicture"));
                if (fileName != null) person.ImageUrl = fileName;
            }

            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            person.PhoneNumber = model.PhoneNumber;
            person.JobTitle = model.JobTitle;
            person.Description = model.Description;
            person.Address = model.Address;
            person.PostalCode = model.PostalCode;
            person.City = model.City;
            person.IsPrivate = model.IsPrivate;

            user.Email = model.Email;
            user.UserName = model.Email;

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profilen har sparats!";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePrivacy(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (person != null && person.IdentityUserId == user.Id)
            {
                person.IsPrivate = !person.IsPrivate;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Profile", new { id = id });
        }

        // --- STATUS (Ersätter Activate/Inactivate) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, bool isActive)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FindAsync(id);

            // Kontrollera ägare
            if (person != null && person.IdentityUserId == user?.Id)
            {
                person.IsActive = isActive;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = isActive
                    ? "Din profil är nu aktiverad igen!"
                    : "Din profil är nu inaktiverad och syns inte för andra.";
            }

            return RedirectToAction("Profile", new { id = id });
        }

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

        // --- UPLOAD CV ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCv(int personId, IFormFile cvFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null || person.Id != personId) return Forbid();

            if (cvFile == null || cvFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vänligen välj en fil att ladda upp.";
                return RedirectToAction("Profile", new { id = personId });
            }

            if (Path.GetExtension(cvFile.FileName).ToLower() != ".pdf")
            {
                TempData["ErrorMessage"] = "Kan enbart ta emot CV i PDF-format.";
                return RedirectToAction("Profile", new { id = personId });
            }

            try
            {
                // Använd hjälpmetoden för PDF
                // Spara i uploads/cvs
                string fileName = await SaveFileAsync(cvFile, Path.Combine("uploads", "cvs"));

                person.CvUrl = "/uploads/cvs/" + fileName;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Ditt CV har laddats upp!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ett fel uppstod när filen skulle sparas.";
            }

            return RedirectToAction("Profile", new { id = personId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCv()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null && !string.IsNullOrEmpty(person.CvUrl))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, person.CvUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                person.CvUrl = null;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Ditt CV har tagits bort.";
            }
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> ExportToXml()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.Persons.Include(p => p.PersonProjects).ThenInclude(pp => pp.Project).Include(p => p.IdentityUser).FirstOrDefaultAsync(p => p.IdentityUserId == userId);
            if (person == null) return NotFound();

            var exportData = new ProfileExportModel
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.IdentityUser?.Email,
                Description = person.Description,
                Skills = person.Skills,
                Education = person.Education,
                Experience = person.Experience,
                Projects = person.PersonProjects.Select(pp => new ProjectExportModel { ProjectName = pp.Project.ProjectName, Description = pp.Project.Description }).ToList()
            };

            var serializer = new XmlSerializer(typeof(ProfileExportModel));
            using (var sw = new StringWriter()) { serializer.Serialize(sw, exportData); return File(Encoding.UTF8.GetBytes(sw.ToString()), "application/xml", $"CV_{person.FirstName}_{person.LastName}.xml"); }
        }

        // --- HJÄLPMETODER (Privat) ---

        private async Task<string> SaveFileAsync(IFormFile file, string folderRelativePath)
        {
            if (file == null || file.Length == 0) return null;

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, folderRelativePath);

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}