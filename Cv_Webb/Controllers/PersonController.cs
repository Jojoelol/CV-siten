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
            // 1. Hantera inloggning och identifiera vems profil som ska visas
            var user = await _userManager.GetUserAsync(User);
            var loggedInPerson = user != null
                ? await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id)
                : null;

            if (!id.HasValue && loggedInPerson == null) return RedirectToAction("Login", "Account");
            int targetId = id ?? loggedInPerson.Id;

            // 2. Hämta person-datan med alla nödvändiga kopplingar
            var person = await _context.Persons
                .Include(p => p.IdentityUser)
                .Include(p => p.PersonProjects)
                    .ThenInclude(pp => pp.Project)
                        .ThenInclude(proj => proj.PersonProjects) // Viktigt för deltagarantal
                .FirstOrDefaultAsync(p => p.Id == targetId);

            if (person == null) return NotFound();

            // 3. Säkerhet: Kontrollera om profilen är privat
            if (person.IsPrivate && user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 4. Hantera ViewCount (räkna endast vid riktiga sidvisningar)
            if (Request.Headers["Accept"].ToString().Contains("text/html"))
            {
                person.ViewCount++;
                await _context.SaveChangesAsync();
            }

            // 5. Spara undan metadata för vyn
            ViewBag.IsOwner = (loggedInPerson != null && targetId == loggedInPerson.Id);
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchString;

            // 6. Sök- och Sorteringslogik för projekt
            if (person.PersonProjects != null)
            {
                var projects = person.PersonProjects.AsQueryable();

                // Filtrering
                if (!string.IsNullOrEmpty(searchString))
                {
                    projects = projects.Where(pp =>
                          pp.Project.ProjectName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                          (pp.Role != null && pp.Role.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    );
                }

                // Avancerad sortering
                projects = sortBy switch
                {
                    "name_asc" => projects.OrderBy(pp => pp.Project.ProjectName),
                    "name_desc" => projects.OrderByDescending(pp => pp.Project.ProjectName),

                    "status_planerat" => projects.OrderByDescending(pp => pp.Project.Status == "Planerat").ThenBy(pp => pp.Project.ProjectName),
                    "status_aktivt" => projects.OrderByDescending(pp => pp.Project.Status == "Aktivt").ThenBy(pp => pp.Project.ProjectName),
                    "status_pausat" => projects.OrderByDescending(pp => pp.Project.Status == "Pausat").ThenBy(pp => pp.Project.ProjectName),
                    "status_avslutat" => projects.OrderByDescending(pp => pp.Project.Status == "Avslutat").ThenBy(pp => pp.Project.ProjectName),

                    "members_asc" => projects.OrderBy(pp => pp.Project.PersonProjects.Count),
                    "members_desc" => projects.OrderByDescending(pp => pp.Project.PersonProjects.Count),

                    "tid" => projects.OrderByDescending(pp => pp.Project.StartDate),
                    _ => projects.OrderBy(pp => pp.Project.ProjectName)
                };

                person.PersonProjects = projects.ToList();
            }

            // 7. Logik för "Liknande personer"
            var others = await _context.Persons
               .Where(p => p.Id != targetId && p.IsActive && !p.IsPrivate)
               .ToListAsync();

            var matches = others
                .Select(p =>
                {
                    var score = CalculateMatchScore(person, p);
                    var percent = Math.Min(score * 10, 100);
                    return new { Person = p, Score = score, MatchPercent = percent };
                })
                .Where(x => x.MatchPercent >= 70)   //Minst 70% matchning
                .OrderByDescending(x => x.MatchPercent)
                .Take(4)                            //Max 4 profiler
                .ToList();

            ViewBag.SimilarPersons = matches;


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

            TempData["SuccessMessage"] = "Profilen har sparats!";
            return View(model);
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
        public async Task<IActionResult> UploadCv(int personId, IFormFile cvFile)
        {
            // 1. Kontrollera om filen ens existerar
            if (cvFile == null || cvFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vänligen välj en fil att ladda upp.";
                return RedirectToAction("Profile", new { id = personId });
            }

            // 2. Kontrollera filändelsen
            var extension = Path.GetExtension(cvFile.FileName).ToLower();
            if (extension != ".pdf")
            {
                TempData["ErrorMessage"] = "Kan enbart ta emot CV i PDF-format.";
                return RedirectToAction("Profile", new { id = personId });
            }

            // 3. Säkerhet: Kontrollera att den inloggade användaren faktiskt äger profilen (IDOR-skydd)
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null || person.Id != personId)
            {
                return Forbid(); // Användaren försöker ladda upp till någon annans profil
            }

            // 4. Spara filen
            try
            {
                string fileName = Guid.NewGuid().ToString() + ".pdf";
                // Säkerställ att sökvägen till mappen finns
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                // 5. Uppdatera databasen
                person.CvUrl = "/uploads/cvs/" + fileName;
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = "Ditt CV har laddats upp!";
            }
            catch (Exception ex)
            {
                // Om något går fel tekniskt (t.ex. rättigheter på servern)
                TempData["ErrorMessage"] = "Ett fel uppstod när filen skulle sparas på servern.";
            }

            return RedirectToAction("Profile", new { id = personId });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCv()
        {
            // Hämta inloggad användare för att förhindra IDOR
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null && !string.IsNullOrEmpty(person.CvUrl))
            {
                // 1. (Valfritt) Ta bort den fysiska filen från wwwroot
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, person.CvUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // 2. Nollställ URL:en i databasen
                person.CvUrl = null;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Ditt CV har tagits bort.";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}