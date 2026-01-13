using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels;
using CV_siten.Services; // <-- Viktig: För att hitta din nya service
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CV_siten.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Startsidan, med sökfunktion och utvalda profiler
        public async Task<IActionResult> Index()
        {
            // Hämta senaste projektet för ViewBag
            ViewBag.SenasteProjekt = await _context.Projects.OrderByDescending(p => p.Id).FirstOrDefaultAsync();

            // Hämta inloggad användares person-koppling
            var userId = _userManager.GetUserId(User);
            var currentPerson = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Grundfilter: Endast aktiva och offentliga profiler 
            var query = _context.Persons.Where(p => p.IsActive && !p.IsPrivate);

            // --- STARTSIDA (Urval/Matchning) ---

            // Om inte inloggad eller saknar profil: Visa utvalda profiler
            if (User.Identity?.IsAuthenticated != true || currentPerson == null)
            {
                ViewBag.SectionTitle = "UTVALDA PROFILER";
                ViewBag.MatchData = new Dictionary<int, int>();
                return View(await query.Take(5).ToListAsync()); //visar 5 profiler när utloggad
            }

            // Matchningslogik för inloggad person (VG-Krav 5)
            // Vi exkluderar den inloggade personen från listan
            var others = await query.Where(p => p.Id != currentPerson.Id).ToListAsync();

            var matches = others.Select(p => {
                // HÄR ANROPAS DIN NYA SERVICE ISTÄLLET FÖR LOKAL KOD
                var score = MatchingService.CalculateMatchScore(currentPerson, p);

                return new { Person = p, Score = score, MatchPercent = Math.Min(score * 10, 100) };
            })
            .Where(x => x.Score >= 2) // Filtrera bort de med mycket låg poäng
            .OrderByDescending(x => x.Score)
            .Take(5) // Max 5 profiler
            .ToList();

            ViewBag.MatchData = matches.ToDictionary(x => x.Person.Id, x => x.MatchPercent);
            ViewBag.SectionTitle = "LIKNANDE PROFILER";

            return View(matches.Select(x => x.Person).ToList());
        }

        public async Task<IActionResult> Search(string search, string skill)
        {
            ViewBag.SearchQuery = search;
            ViewBag.SkillQuery = skill;
            ViewBag.SectionTitle = "SÖKRESULTAT";
            ViewBag.MatchData = new Dictionary<int, int>(); // Tom i sökresultat

            var personQuery = _context.Persons.Where(p => p.IsActive);

            if (User.Identity?.IsAuthenticated != true)
                personQuery = personQuery.Where(p => !p.IsPrivate);

            // Söklogik för namn
            if (!string.IsNullOrEmpty(search))
            {
                var parts = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    personQuery = personQuery.Where(p => p.FirstName.Contains(part) || p.LastName.Contains(part));
                }
            }

            // Söklogik för kompetens
            if (!string.IsNullOrEmpty(skill))
            {
                personQuery = personQuery.Where(p => p.Skills.Contains(skill) ||
                                                    p.Education.Contains(skill) ||
                                                    p.JobTitle.Contains(skill));
            }

            var personResult = await personQuery.ToListAsync();
            ViewBag.PersonResult = personResult;

            // Projekt-sökning
            var projektQuery = _context.Projects.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                projektQuery = projektQuery.Where(p => p.ProjectName.Contains(search));
            }

            var projektResult = await projektQuery.ToListAsync();

            // Returnera vyn SearchResult med projektlistan som modell (och personlistan i ViewBag)
            return View("SearchResult", projektResult);
        }

        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}