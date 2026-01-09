using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Controllers
{
    public class ProjectController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProjectController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // --- VISA ALLA PROJEKT ---
        public async Task<IActionResult> Index()
        {
            var allaProjekt = await _context.Projects.ToListAsync();
            return View(allaProjekt);
        }

        // --- PROJEKTDETALJER ---
        public async Task<IActionResult> ProjectDetails(int id)
        {
            var project = await _context.Projects
                .Include(p => p.PersonProjects).ThenInclude(pp => pp.Person)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isOwner = false;
            bool isParticipant = false;
            int? currentPersonId = null;

            if (user != null)
            {
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                if (person != null)
                {
                    currentPersonId = person.Id;
                    isParticipant = project.PersonProjects.Any(pp => pp.PersonId == person.Id);
                    isOwner = project.OwnerId == person.Id;
                }
            }

            ViewBag.IsOwner = isOwner;
            ViewBag.IsParticipant = isParticipant;
            ViewBag.CurrentPersonId = currentPersonId;

            return View(project);
        }

        // --- SKAPA NYTT PROJEKT (GET) ---
        [Authorize]
        [HttpGet]
        public IActionResult AddProject() => View();

        // --- SKAPA NYTT PROJEKT (POST) ---


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(Project model, IFormFile? imageFile, IFormFile? zipFile, string Role)
        {


            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

                if (person == null) return RedirectToAction("Index", "Home");

                // 2. Hantera Projektbild
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/projects");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                    using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    model.ImageUrl = fileName;
                }

                // 3. Hantera ZIP-fil
                if (zipFile != null && zipFile.Length > 0)
                {
                    string zipFileName = Guid.NewGuid().ToString() + Path.GetExtension(zipFile.FileName);
                    string zipUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
                    if (!Directory.Exists(zipUploadPath)) Directory.CreateDirectory(zipUploadPath);
                    using (var stream = new FileStream(Path.Combine(zipUploadPath, zipFileName), FileMode.Create))
                    {
                        await zipFile.CopyToAsync(stream);
                    }
                    model.ZipUrl = zipFileName;
                }

                // 4. Sätt ägare och spara projektet
                // model.Type och model.Status kommer automatiskt från din nya vy här!
                model.OwnerId = person.Id;
                _context.Projects.Add(model);
                await _context.SaveChangesAsync();

                // 5. Skapa kopplingen till personen
                _context.PersonProjects.Add(new PersonProject
                {
                    PersonId = person.Id,
                    ProjectId = model.Id,
                    Role = Role ?? "Projektägare"
                });

                await _context.SaveChangesAsync();

                // 6. BEHÅLL DIN POPUP-LOGIK
                ViewBag.ShowSuccessPopup = true;

                return View(model);
            }

            // Om valideringen misslyckas skickas vi tillbaka till vyn med felen
            return View(model);
        }
        // --- REDIGERA PROJEKT (GET) ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            // Säkerhetskontroll: Endast ägaren får redigera
            if (project == null || person == null || project.OwnerId != person.Id)
            {
                return Forbid();
            }

            var viewModel = new EditProjectViewModel
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Type = project.Type,
                Status = project.Status,
                ImageUrl = project.ImageUrl,
                ZipUrl = project.ZipUrl
            };

            // VIKTIGT: Vi pekar ut den specifika vyn EditProject.cshtml
            return View("EditProject", viewModel);
        }

        // --- REDIGERA PROJEKT (POST) ---
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            if (!ModelState.IsValid) return View("EditProject", model);

            var project = await _context.Projects.FindAsync(model.Id);
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (project == null || person == null || project.OwnerId != person.Id) return Forbid();

            // Uppdatera Bild om ny fil skickats
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/projects", fileName);
                using (var stream = new FileStream(path, FileMode.Create)) { await model.ImageFile.CopyToAsync(stream); }
                project.ImageUrl = fileName;
            }

            // Uppdatera ZIP-fil om ny fil skickats
            if (model.ZipFile != null && model.ZipFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ZipFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects", fileName);
                using (var stream = new FileStream(path, FileMode.Create)) { await model.ZipFile.CopyToAsync(stream); }
                project.ZipUrl = fileName;
            }

            // Uppdatera textfälten
            project.ProjectName = model.ProjectName;
            project.Description = model.Description ?? "";
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.Type = model.Type;
            project.Status = model.Status;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Projektet har uppdaterats!";
            return RedirectToAction(nameof(ProjectDetails), new { id = project.Id });
        }

        // --- UPPDATERA BESKRIVNING (Snabb-redigering via AJAX/JS) ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateDescription(int id, string description)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            var project = await _context.Projects.FindAsync(id);

            if (project == null || person == null || project.OwnerId != person.Id) return Forbid();

            project.Description = description;
            await _context.SaveChangesAsync();
            return RedirectToAction("ProjectDetails", new { id = id });
        }

        // --- GÅ MED I PROJEKT (GET) ---
        [Authorize]
        // 1. Visar listan över projekt man kan gå med i
        [HttpGet]
        public async Task<IActionResult> JoinProject()
        {
            // Hämta inloggad användare
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            // Hämta alla projekt som personen INTE redan är med i
            var projects = await _context.Projects
                .Include(p => p.Owner)
                .Where(p => !p.PersonProjects.Any(pp => pp.PersonId == person.Id))
                .ToListAsync();

            return View(projects);
        }

        // 2. Hanterar när man klickar på "Gå med" i modalen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int projectId, string role)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null)
            {
                // Skapa kopplingen i join-tabellen
                var personProject = new PersonProject
                {
                    PersonId = person.Id,
                    ProjectId = projectId,
                    Role = role
                };

                _context.PersonProjects.Add(personProject);
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = "Du har nu gått med i projektet!";
            }

            // Skicka tillbaka användaren till sin egen profil
            return RedirectToAction("Profile", "Person");
        }

        // --- LÄMNA PROJEKT ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LeaveProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            var project = await _context.Projects.FindAsync(id);

            if (person != null && project != null)
            {
                if (project.OwnerId == person.Id)
                {
                    TempData["Error"] = "Som ägare kan du inte lämna projektet. Du måste radera det.";
                    return RedirectToAction("ProjectDetails", new { id = id });
                }

                var connection = await _context.PersonProjects
                    .FirstOrDefaultAsync(pp => pp.ProjectId == id && pp.PersonId == person.Id);

                if (connection != null)
                {
                    _context.PersonProjects.Remove(connection);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("ProjectDetails", new { id = id });
        }

        // --- RADERA PROJEKT ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            var project = await _context.Projects.Include(p => p.PersonProjects).FirstOrDefaultAsync(p => p.Id == id);

            if (project == null || person == null || project.OwnerId != person.Id) return Forbid();

            _context.PersonProjects.RemoveRange(project.PersonProjects);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Projektet har raderats helt.";
            return RedirectToAction("Profile", "Person", new { id = person.Id });
        }

        public async Task<IActionResult> AllProjects()
        {
            // Vi hämtar alla projekt och inkluderar ägaren för att kunna skriva ut namnet
            var projects = await _context.Projects
                .Include(p => p.Owner)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(projects);
        }
    }
}