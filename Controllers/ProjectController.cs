using CV_siten.Data;
using CV_siten.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Visa alla projekt (Lista)
        public async Task<IActionResult> Index()
        {
            var allaProjekt = await _context.Projekt.ToListAsync();
            return View(allaProjekt);
        }

        // Visa ett specifikt projekt och dess deltagare
        public async Task<IActionResult> Details(int id)
        {
            var projekt = await _context.Projekt
                .Include(p => p.PersonProjekt)        // Inkludera kopplingstabellen
                    .ThenInclude(pp => pp.Person)     // Inkludera personen via kopplingen
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projekt == null)
            {
                return NotFound();
            }

            return View(projekt);
        }

        [HttpGet]
        public IActionResult AddProject()
        {
            // Detta öppnar filen Views/Projekt/AddProject.cshtml
            return View();
        }
    }
}