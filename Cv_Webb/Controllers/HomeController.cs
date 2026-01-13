using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels;
using CV_siten.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CV_siten.Controllers
{
    // Hanterar startsidan, matchningslogik och sökfunktionalitet
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Dependency injection av databas och användarhanterare
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Hämtar senaste projektet för att visa i "Hero"-sektionen
            ViewBag.SenasteProjekt = await _context.Projects.OrderByDescending(p => p.Id).FirstOrDefaultAsync();

            // Identifiera inloggad användare och hämta tillhörande profil
            var userId = _userManager.GetUserId(User);
            var currentPerson = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Grundfråga: Vi är bara intresserade av profiler som är aktiva och inte privata
            var query = _context.Persons.Where(p => p.IsActive && !p.IsPrivate);

            // Scenario 1: Besökaren är inte inloggad eller saknar egen profil
            // Då visar vi bara ett generellt urval (de 5 första) för att fylla sidan
            if (User.Identity?.IsAuthenticated != true || currentPerson == null)
            {
                ViewBag.SectionTitle = "UTVALDA PROFILER";
                ViewBag.MatchData = new Dictionary<int, int>();
                return View(await query.Take(5).ToListAsync());
            }

            // Scenario 2: Besökaren är inloggad
            // Hämta alla andra kandidater utom den inloggade själv
            var others = await query.Where(p => p.Id != currentPerson.Id).ToListAsync();

            // Beräkna matchningspoäng via MatchingService och omvandla till procent för GUI
            var matches = others.Select(p => {
                var score = MatchingService.CalculateMatchScore(currentPerson, p);
                return new { Person = p, Score = score, MatchPercent = Math.Min(score * 10, 100) };
            })
            .Where(x => x.Score >= 2)        // Filtrera bort låg relevans
            .OrderByDescending(x => x.Score) // Mest relevanta först
            .Take(5)
            .ToList();

            // Skicka matchningsdata separat via ViewBag
            ViewBag.MatchData = matches.ToDictionary(x => x.Person.Id, x => x.MatchPercent);
            ViewBag.SectionTitle = "LIKNANDE PROFILER";

            return View(matches.Select(x => x.Person).ToList());
        }

        public async Task<IActionResult> Search(string search, string skill)
        {
            // Sparar sökorden i ViewBag så de står kvar i input-fälten
            ViewBag.SearchQuery = search;
            ViewBag.SkillQuery = skill;
            ViewBag.SectionTitle = "SÖKRESULTAT";
            ViewBag.MatchData = new Dictionary<int, int>(); // Ingen matchning vid sökning

            // Börja med alla aktiva profiler
            var personQuery = _context.Persons.Where(p => p.IsActive);

            // Säkerhet: Om användaren inte är inloggad får de ALDRIG se privata profiler
            if (User.Identity?.IsAuthenticated != true)
                personQuery = personQuery.Where(p => !p.IsPrivate);

            // Filtrera på namn (hanterar både för- och efternamn genom split)
            if (!string.IsNullOrEmpty(search))
            {
                var parts = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    personQuery = personQuery.Where(p => p.FirstName.Contains(part) || p.LastName.Contains(part));
                }
            }

            // Filtrera på kompetens (letar i Skills, Education och JobTitle)
            if (!string.IsNullOrEmpty(skill))
            {
                personQuery = personQuery.Where(p => p.Skills.Contains(skill) ||
                                                    p.Education.Contains(skill) ||
                                                    p.JobTitle.Contains(skill));
            }

            var personResult = await personQuery.ToListAsync();
            ViewBag.PersonResult = personResult;

            // Sökning efter projekt (separat lista)
            var projektQuery = _context.Projects.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                projektQuery = projektQuery.Where(p => p.ProjectName.Contains(search));
            }

            var projektResult = await projektQuery.ToListAsync();

            // Returnera sökresultatsvyn. Notera att Projekt är Model, Personer ligger i ViewBag.
            return View("SearchResult", projektResult);
        }

        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}