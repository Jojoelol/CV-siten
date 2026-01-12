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
            // Vi behåller endast de kopplingar där personen är aktiv
            project.PersonProjects = project.PersonProjects
                .Where(pp => pp.Person != null && pp.Person.IsActive)
                .ToList();
            // ----------------------------------------------------

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
                    // Här kollar vi nu mot den filtrerade listan
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
        public async Task<IActionResult> AddProject(AddProjectViewModel model)
        {
            // 1. Logisk validering: Slutdatum får inte vara före startdatum
            if (model.EndDate.HasValue && model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Slutdatum kan inte vara före startdatum.");
            }

            // Kontrollera om ViewModel-attributen ([Required], [MaxLength] etc) är uppfyllda
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

                if (person == null) return RedirectToAction("Index", "Home");

                // 2. Mappa data från ViewModel till den faktiska databasmodellen (Project)
                var newProject = new Project
                {
                    ProjectName = model.ProjectName,
                    Description = model.Description,
                    StartDate = new DateTimeOffset(model.StartDate.Value),
                    EndDate = model.EndDate.HasValue ? new DateTimeOffset(model.EndDate.Value) : null,
                    Type = model.Type ?? "",
                    Status = model.Status,
                    OwnerId = person.Id
                };

                // 3. Hantera Projektbild
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/projects");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    newProject.ImageUrl = fileName;
                }

                // 4. Hantera ZIP-fil
                if (model.ZipFile != null && model.ZipFile.Length > 0)
                {
                    string zipFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ZipFile.FileName);
                    string zipUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
                    if (!Directory.Exists(zipUploadPath)) Directory.CreateDirectory(zipUploadPath);

                    using (var stream = new FileStream(Path.Combine(zipUploadPath, zipFileName), FileMode.Create))
                    {
                        await model.ZipFile.CopyToAsync(stream);
                    }
                    newProject.ZipUrl = zipFileName;
                }

                try
                {
                    // 5. Spara projektet
                    _context.Projects.Add(newProject);

                    // 6. Skapa kopplingen i PersonProject (ägaren blir automatiskt medlem)
                    _context.PersonProjects.Add(new PersonProject
                    {
                        PersonId = person.Id,
                        Project = newProject,
                        Role = model.Role ?? "Projektägare"
                    });

                    await _context.SaveChangesAsync();

                    ViewBag.ShowSuccessPopup = true;
                    return View(model);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Ett fel uppstod när projektet sparades i databasen.");
                }
            }

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

            // Uppdatera Bild
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/projects", fileName);
                using (var stream = new FileStream(path, FileMode.Create)) { await model.ImageFile.CopyToAsync(stream); }
                project.ImageUrl = fileName;
            }

            // Uppdatera ZIP-fil
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

        // --- UPPDATERA BESKRIVNING (AJAX) ---
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

        // --- GÅ MED I PROJEKT (GET - Lista) ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> JoinProject()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            // Hämta projekt där personen INTE är med
            var projects = await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.PersonProjects)
                .Where(p => !p.PersonProjects.Any(pp => pp.PersonId == person.Id))
                .ToListAsync();

            return View(projects);
        }

        // --- GÅ MED I PROJEKT (POST - Från Lista) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int projectId, string role)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null)
            {
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
            return RedirectToAction("Profile", "Person");
        }

        // --- GÅ MED I PROJEKT (POST - Från Detaljvy) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> JoinProject(int projectId, string role)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null)
            {
                var isAlreadyMember = await _context.PersonProjects
                    .AnyAsync(pp => pp.PersonId == person.Id && pp.ProjectId == projectId);

                if (!isAlreadyMember)
                {
                    var newParticipant = new PersonProject
                    {
                        PersonId = person.Id,
                        ProjectId = projectId,
                        Role = role
                    };

                    _context.PersonProjects.Add(newParticipant);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Välkommen till projektet! Din anmälan är nu registrerad.";
                }
            }

            return RedirectToAction("ProjectDetails", new { id = projectId });
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

        // --- LISTA ALLA PROJEKT (Sök & Sortering) ---
        [HttpGet]
        public async Task<IActionResult> AllProjects(string searchString, string sortBy)
        {
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentSort = sortBy;

            var query = _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.PersonProjects)
                    .ThenInclude(pp => pp.Person) // <--- VIKTIGT: Behövs för att visa deltagare och kolla sekretess!
                .AsQueryable();

            // Filtrering
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.ProjectName.Contains(searchString) ||
                                         p.Owner.FirstName.Contains(searchString) ||
                                         p.Owner.LastName.Contains(searchString));
            }

            // Sortering
            query = sortBy switch
            {
                "name_desc" => query.OrderByDescending(p => p.ProjectName),
                "status_aktivt" => query.OrderBy(p => p.Status != "Aktivt").ThenBy(p => p.ProjectName),
                "status_avslutat" => query.OrderBy(p => p.Status != "Avslutat").ThenBy(p => p.ProjectName),
                "members_desc" => query.OrderByDescending(p => p.PersonProjects.Count),
                "tid" => query.OrderByDescending(p => p.StartDate),
                _ => query.OrderBy(p => p.ProjectName),
            };

            var projects = await query.ToListAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                ViewBag.CurrentPersonId = person?.Id;
            }

            return View(projects);
        }
    }
}