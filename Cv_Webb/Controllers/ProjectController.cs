using CV_siten.Data.Data;
using CV_siten.Data.Models;
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

                    // ÄNDRING: Kontrollera mot OwnerId i databasen
                    isOwner = project.OwnerId == person.Id;
                }
            }

            ViewBag.IsOwner = isOwner;
            ViewBag.IsParticipant = isParticipant;
            ViewBag.CurrentPersonId = currentPersonId;

            return View(project);
        }

        // --- SKAPA NYTT PROJEKT ---
        [Authorize]
        [HttpGet]
        public IActionResult AddProject() => View();

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(Project model, IFormFile? imageFile) // Lägg till imageFile här
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                if (person == null) return RedirectToAction("Index", "Home");

                // --- BILDHANTERING ---
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Skapa ett unikt filnamn
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    // Sökväg: wwwroot/images/projects
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "projects");

                    // Skapa mappen om den inte finns
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Spara den relativa sökvägen i databasen
                    model.ImageUrl = fileName;
                }

                model.OwnerId = person.Id;
                _context.Projects.Add(model);
                await _context.SaveChangesAsync();

                // Skapa koppling i PersonProjects
                _context.PersonProjects.Add(new PersonProject { PersonId = person.Id, ProjectId = model.Id });
                await _context.SaveChangesAsync();

                ViewBag.ShowSuccessPopup = true;
                return View(model);
            }
            return View(model);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateDescription(int id, string description)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            var project = await _context.Projects.FindAsync(id);

            // KONTROLL: Finns projektet och ägs det av den inloggade personen?
            if (project == null || person == null || project.OwnerId != person.Id)
            {
                return Forbid(); // Ger "403 Forbidden" om man försöker ändra någon annans projekt
            }

            project.Description = description;
            await _context.SaveChangesAsync();

            return RedirectToAction("ProjectDetails", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> JoinProject()
        {
            // 1. Hämta den inloggade användarens person-objekt
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .Include(p => p.PersonProjects)
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null) return NotFound();

            // 2. Hitta ID:n på de projekt användaren redan är med i
            var joinedProjectIds = person.PersonProjects.Select(pp => pp.ProjectId).ToList();

            // 3. Hämta ALLA projekt som användaren inte är med i ännu
            var availableProjects = await _context.Projects
           .Include(p => p.Owner) // Se till att Owner-objektet laddas
           .Where(p => !joinedProjectIds.Contains(p.Id))
           .ToListAsync();

            return View(availableProjects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int projectId)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null)
            {
                // Skapa den nya kopplingen mellan person och projekt
                var newConnection = new PersonProject
                {
                    PersonId = person.Id,
                    ProjectId = projectId
                };

                _context.PersonProjects.Add(newConnection);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Du har nu gått med i projektet!";
            }

            return RedirectToAction("Profile", "Person", new { id = person?.Id });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LeaveProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            var project = await _context.Projects.FindAsync(id);

            if (person != null && project != null)
            {
                // KONTROLL: Ägaren bör inte kunna lämna sitt eget projekt (de måste radera det istället)
                if (project.OwnerId == person.Id)
                {
                    TempData["Error"] = "Som ägare kan du inte lämna projektet. Du måste radera det helt om du vill avsluta.";
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

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            var project = await _context.Projects
                .Include(p => p.PersonProjects) // Inkludera kopplingarna för säker radering
                .FirstOrDefaultAsync(p => p.Id == id);

            // SÄKERHETSKONTROLL: Endast ägaren får radera
            if (project == null || person == null || project.OwnerId != person.Id)
            {
                return Forbid();
            }

            // Ta bort kopplingar först (om det inte sker automatiskt i databasen)
            _context.PersonProjects.RemoveRange(project.PersonProjects);

            // Ta bort projektet
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Projektet har raderats helt.";
            return RedirectToAction("Profile", "Person", new { id = person.Id });
        }
    }
}

