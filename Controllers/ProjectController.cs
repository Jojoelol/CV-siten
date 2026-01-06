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

        [HttpPost]
        public async Task<IActionResult> UploadCv(IFormFile cvFile, int personId)
        {
            if (cvFile != null && cvFile.Length > 0)
            {
                // Krav: Validera filtyp (Endast PDF är säkrast)
                var extension = Path.GetExtension(cvFile.FileName).ToLower();
                if (extension != ".pdf")
                {
                    return BadRequest("Endast PDF-filer är tillåtna.");
                }

                // Krav: Skapa unikt filnamn med Guid för att undvika överskrivning
                string fileName = Guid.NewGuid().ToString() + extension;
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
                string filePath = Path.Combine(folderPath, fileName);

                // Krav: Spara filen fysiskt på servern
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                // Krav: Spara endast sökvägen (strängen) i databasen
                var person = await _context.Persons.FindAsync(personId);
                if (person != null)
                {
                    person.CvUrl = "/uploads/cvs/" + fileName;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Profile", "Account", new { id = personId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCv(int personId)
        {
            var person = await _context.Persons.FindAsync(personId);

            if (person != null && !string.IsNullOrEmpty(person.CvUrl))
            {
                // 1. Hitta den fysiska sökvägen till filen på servern
                // Vi tar bort det inledande '/' för att Path.Combine ska fungera rätt med wwwroot
                var relativePath = person.CvUrl.TrimStart('/');
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                // 2. Radera filen från hårddisken om den existerar
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // 3. Nollställ referensen i databasen
                person.CvUrl = null;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile", "Account", new { id = personId });
        }

        public async Task<IActionResult> ProjectDetails(int id)
        {
            var projekt = await _context.Projekt
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projekt == null) return NotFound();

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