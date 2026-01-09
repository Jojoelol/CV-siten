using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using System.IO;
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

        // --- VISA PROFIL (G-krav 1, 6 & VG-krav 1, 5) ---
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

            if (Request.Headers["Accept"].ToString().Contains("text/html"))
            {
                person.ViewCount++;
                await _context.SaveChangesAsync();
            }

            if (person.IsPrivate && user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.IsOwner = (loggedInPerson != null && targetId == loggedInPerson.Id);

            var others = await _context.Persons
               .Where(p => p.Id != targetId && p.IsActive && !p.IsPrivate)
               .ToListAsync();

            var bestMatch = others
                .Select(p => new { Person = p, Score = CalculateMatchScore(person, p) })
                .Where(x => x.Score >= 2) // Tröskel för relevans
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (bestMatch != null)
            {
                ViewBag.SimilarPerson = bestMatch.Person;
                // RÄKNA UT PROCENTEN HÄR OCH SKICKA TILL VY:
                ViewBag.MatchScore = Math.Min(bestMatch.Score * 10, 100);
            }

            ViewBag.SimilarPerson = bestMatch?.Person;

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


        private bool FuzzySkillMatch(string a, string b)
        {
            a = a.ToLower().Trim();
            b = b.ToLower().Trim();
            if (a == b || a.Contains(b) || b.Contains(a)) return true;

            var synonyms = new Dictionary<string, string[]>
            {
                { "javascript", new[] { "js" } }, { "js", new[] { "javascript" } },
                { "c#", new[] { "c sharp", "c-sharp" } }, { "c sharp", new[] { "c#", "c-sharp" } },
                { "react", new[] { "react.js", "reactjs" } }, { "sql", new[] { "t-sql", "mysql" } }
            };

            if (synonyms.ContainsKey(a) && synonyms[a].Contains(b)) return true;
            if (synonyms.ContainsKey(b) && synonyms[b].Contains(a)) return true;
            return false;
        }

        private int CalculateMatchScore(Person a, Person b)
        {
            int score = 0;
            if (!string.IsNullOrEmpty(a.JobTitle) && a.JobTitle.Equals(b.JobTitle, StringComparison.OrdinalIgnoreCase)) score += 3;
            if (!string.IsNullOrEmpty(a.Skills) && !string.IsNullOrEmpty(b.Skills))
            {
                var skillsA = a.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower());
                var skillsB = b.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower());
                foreach (var skA in skillsA) { foreach (var skB in skillsB) { if (FuzzySkillMatch(skA, skB)) { score += 2; break; } } }
            }
            if (!string.IsNullOrEmpty(a.Education) && a.Education.Equals(b.Education, StringComparison.OrdinalIgnoreCase)) score += 2;
            return score;
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

        // --- REDIGERA PROFIL (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null) return NotFound();

            if (model.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "ProfilePicture");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create)) { await model.ImageFile.CopyToAsync(stream); }
                person.ImageUrl = fileName;
            }

            person.FirstName = model.FirstName; person.LastName = model.LastName; person.PhoneNumber = model.PhoneNumber;
            person.JobTitle = model.JobTitle; person.Description = model.Description; person.Address = model.Address;
            person.PostalCode = model.PostalCode; person.City = model.City; person.IsPrivate = model.IsPrivate;
            user.Email = model.Email; user.UserName = model.Email;
            await _userManager.UpdateAsync(user); await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profilen har uppdaterats!";
            return RedirectToAction(nameof(Profile), new { id = person.Id });
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
        [HttpPost]
        [ValidateAntiForgeryToken] // Bra för säkerhet/VG
        public async Task<IActionResult> Inactivate(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FindAsync(id);

            // Kontrollera att det är ägaren (Viktigt för VG-nivå!)
            if (person != null && person.IdentityUserId == user?.Id)
            {
                person.IsActive = false;
                _context.Update(person);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Din profil är nu inaktiverad och syns inte för andra.";
            }

            return RedirectToAction("Profile", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FindAsync(id);

            if (person != null && person.IdentityUserId == user?.Id)
            {
                person.IsActive = true;
                _context.Update(person);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Din profil är nu aktiverad igen!";
            }

            return RedirectToAction("Profile", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileField(int id, string fieldName, string fieldValue)
        {
            var person = await _context.Persons.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (person == null || person.IdentityUserId != user.Id) return Forbid();
            switch (fieldName) { case "Skills": person.Skills = fieldValue; break; case "Education": person.Education = fieldValue; break; case "Experience": person.Experience = fieldValue; break; }
            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", new { id = id });
        }

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
                person.CvUrl = "/uploads/cvs/" + fileName; await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Profile));
        }


        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null) return NotFound();

            // Skickar vidare till Profile-metoden men med den inloggades ID
            return RedirectToAction("Profile", new { id = person.Id });
        }
    }
}