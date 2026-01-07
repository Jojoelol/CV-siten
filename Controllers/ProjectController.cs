using CV_siten.Data.Data;
using CV_siten.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CV_siten.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- VISA ALLA PROJEKT ---
        // Listar alla projekt på siten
        public async Task<IActionResult> Index()
        {
            var allaProjekt = await _context.Projects.ToListAsync();
            return View(allaProjekt);
        }

        // --- PROJEKTDETALJER ---
        // Visar info om ett projekt och vilka personer som deltar
        public async Task<IActionResult> Details(int id)
        {
            var projekt = await _context.Projects
                .Include(p => p.PersonProjects)       // Kopplingstabellen
                    .ThenInclude(pp => pp.Person)    // Deltagarna
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projekt == null)
            {
                return NotFound();
            }

            return View(projekt);
        }

        // --- SKAPA NYTT PROJEKT ---
        [Authorize]
        [HttpGet]
        public IActionResult AddProject()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }
    }
}