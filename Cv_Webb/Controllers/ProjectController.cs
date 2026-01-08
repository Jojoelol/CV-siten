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
                .Include(p => p.PersonProjects)
                    .ThenInclude(pp => pp.Person)
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
                    // Kontrollera om användaren är med i projektet
                    isParticipant = project.PersonProjects.Any(pp => pp.PersonId == person.Id);

                    // Här definierar vi "Owner" (t.ex. den som skapade det eller har flaggan IsOwner)
                    // För enkelhetens skull i detta exempel använder vi samma logik som tidigare
                    isOwner = isParticipant;
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
        public async Task<IActionResult> AddProject(Project model, string Role)
        {
            if (ModelState.IsValid)
            {
                // 1. Hämta den inloggade användaren via Identity
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                // 2. Hitta kopplad Person i databasen via IdentityUserId
                var person = await _context.Persons
                    .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

                if (person == null)
                {
                    // Om personen inte hittas, hantera felet (t.ex. skicka till profilskapande)
                    return RedirectToAction("Index", "Home");
                }

                // 3. Spara själva projektet
                _context.Projects.Add(model);
                await _context.SaveChangesAsync(); // Genererar model.Id

                // 4. Skapa kopplingen mellan Person och Projekt (Krav 4e)
                // Om du har ett fält för 'Roll' i din PersonProject-tabell kan du spara string Role här
                var personProject = new PersonProject
                {
                    PersonId = person.Id,
                    ProjectId = model.Id
                    // Om din modell tillåter: Role = Role 
                };

                _context.PersonProjects.Add(personProject);
                await _context.SaveChangesAsync();

                // --- HÄR AKTIVERAS POPUPEN ---
                // Vi sätter ViewBag till true. Din AddProject.cshtml läser av detta 
                // via det dolda elementet #popup-data och visar rutan via site.js.
                ViewBag.ShowSuccessPopup = true;

                return View(model);
            }

            // Om valideringen misslyckas (t.ex. saknat namn), visa vyn igen med felmeddelanden
            return View(model);
        }

        //UPPDATERA PROJEKTINFO
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateDescription(int id, string description)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                project.Description = description;
                await _context.SaveChangesAsync();
            }

            // Skicka tillbaka användaren till samma sida
            return RedirectToAction("ProjectDetails", new { id = id });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LeaveProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null)
            {
                // Hitta kopplingen mellan personen och projektet
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
    }
}

