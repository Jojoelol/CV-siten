using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CV_siten.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger; _context = context; _userManager = userManager;
        }

        public async Task<IActionResult> Index(string search, string skill)
        {
            // Hämta senaste projektet för ViewBag (G-krav)
            ViewBag.SenasteProjekt = await _context.Projects.OrderByDescending(p => p.Id).FirstOrDefaultAsync();

            // Hämta inloggad användare och person-koppling
            var userId = _userManager.GetUserId(User);
            var currentPerson = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Grundfilter: Visa endast profiler som är AKTIVA och INTE PRIVATA (VG-krav sekretess)
            var query = _context.Persons.Where(p => p.IsActive && !p.IsPrivate);

            // --- SÖKLOGIK (VG-Krav 6) ---
            bool isSearching = false;
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => (p.FirstName + " " + p.LastName).Contains(search));
                isSearching = true;
            }
            if (!string.IsNullOrEmpty(skill))
            {
                query = query.Where(p => p.Skills.Contains(skill));
                isSearching = true;
            }

            // Om användaren söker, visa sökresultaten direkt
            if (isSearching)
            {
                ViewBag.SectionTitle = "SÖKRESULTAT";
                ViewBag.MatchData = new Dictionary<int, int>(); // Ingen matchningsscore vid vanlig sökning
                var searchResults = await query.ToListAsync();
                return View(searchResults);
            }

            // --- LOGIK FÖR STARTSIDAN (Matchning/Urval) ---

            // Om inte inloggad eller ingen personprofil finns: Visa utvalda profiler
            if (!User.Identity.IsAuthenticated || currentPerson == null)
            {
                ViewBag.SectionTitle = "UTVALDA PROFILER";
                ViewBag.MatchData = new Dictionary<int, int>();
                var urvalCV = await query.Take(3).ToListAsync();
                return View(urvalCV);
            }

            // Matchningar för inloggad person (VG-Krav 5)
            var others = await query.Where(p => p.Id != currentPerson.Id).ToListAsync();
            var matches = others.Select(p => {
                var score = CalculateMatchScore(currentPerson, p);
                return new { Person = p, Score = score, MatchPercent = Math.Min(score * 10, 100) };
            }).Where(x => x.Score >= 5).OrderByDescending(x => x.Score).Take(3).ToList();

            ViewBag.MatchData = matches.ToDictionary(x => x.Person.Id, x => x.MatchPercent);
            ViewBag.SectionTitle = "LIKNANDE PROFILER";

            return View(matches.Select(x => x.Person).ToList());
        }

        private bool FuzzySkillMatch(string a, string b)
        {
            a = a.ToLower().Trim(); b = b.ToLower().Trim();
            if (a == b || a.Contains(b) || b.Contains(a)) return true;
            var synonyms = new Dictionary<string, string[]> { { "javascript", new[] { "js" } }, { "js", new[] { "javascript" } }, { "c#", new[] { "c sharp" } } };
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
                var skillsA = a.Skills.Split(',').Select(s => s.Trim().ToLower());
                var skillsB = b.Skills.Split(',').Select(s => s.Trim().ToLower());
                foreach (var skA in skillsA) { foreach (var skB in skillsB) { if (FuzzySkillMatch(skA, skB)) { score += 2; break; } } }
            }
            return score;
        }

        public async Task<IActionResult> Search(string search, string skill)
        {
            var query = _context.Persons.Where(p => p.IsActive);
            if (!User.Identity.IsAuthenticated) query = query.Where(p => !p.IsPrivate);
            if (!string.IsNullOrEmpty(search)) { string s = search.ToUpper(); query = query.Where(p => p.FirstName.ToUpper().Contains(s) || p.LastName.ToUpper().Contains(s)); }
            if (!string.IsNullOrEmpty(skill)) { string sk = skill.ToUpper(); query = query.Where(p => p.Skills.ToUpper().Contains(sk) || p.Education.ToUpper().Contains(sk) || p.JobTitle.ToUpper().Contains(sk)); }

            ViewBag.personResult = await query.ToListAsync();
            var projektResult = await _context.Projects.Where(p => string.IsNullOrEmpty(search) || p.ProjectName.ToUpper().Contains(search.ToUpper())).ToListAsync();
            return View("SearchResult", projektResult);
        }

        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}