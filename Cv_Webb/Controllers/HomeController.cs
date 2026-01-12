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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string search, string skill)
        {
            // Hämta senaste projektet för ViewBag (G-krav)
            ViewBag.SenasteProjekt = await _context.Projects.OrderByDescending(p => p.Id).FirstOrDefaultAsync();

            // Hämta inloggad användares person-koppling
            var userId = _userManager.GetUserId(User);
            var currentPerson = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Grundfilter: Endast aktiva och offentliga profiler (VG-krav sekretess)
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
                // Utökad sökning för att inkludera utbildning och titel likt din Search-metod
                query = query.Where(p => p.Skills.Contains(skill) || p.Education.Contains(skill) || p.JobTitle.Contains(skill));
                isSearching = true;
            }

            // Visa sökresultat direkt om sökning utförts
            if (isSearching)
            {
                ViewBag.SectionTitle = "SÖKRESULTAT";
                ViewBag.MatchData = new Dictionary<int, int>();
                return View(await query.ToListAsync());
            }

            // --- STARTSIDA (Urval/Matchning) ---

            // Om inte inloggad eller saknar profil: Visa utvalda profiler
            if (User.Identity?.IsAuthenticated != true || currentPerson == null)
            {
                ViewBag.SectionTitle = "UTVALDA PROFILER";
                ViewBag.MatchData = new Dictionary<int, int>();
                return View(await query.Take(3).ToListAsync());
            }

            // Matchningslogik för inloggad person (VG-Krav 5) - Nu synkad med PersonController
            var others = await query.Where(p => p.Id != currentPerson.Id).ToListAsync();

            var matches = others.Select(p => {
                var score = CalculateMatchScore(currentPerson, p);
                return new { Person = p, Score = score, MatchPercent = Math.Min(score * 10, 100) };
            })
            .Where(x => x.Score >= 2) // Ändrat från 5 till 2 för att matcha PersonController
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToList();

            ViewBag.MatchData = matches.ToDictionary(x => x.Person.Id, x => x.MatchPercent);
            ViewBag.SectionTitle = "LIKNANDE PROFILER";

            return View(matches.Select(x => x.Person).ToList());
        }

        // --- SYNKOADE HJÄLPMETODER (Identiska med PersonController) ---

        private bool FuzzySkillMatch(string a, string b)
        {
            a = a.ToLower().Trim();
            b = b.ToLower().Trim();
            if (a == b || a.Contains(b) || b.Contains(a)) return true;

            var synonyms = new Dictionary<string, string[]>
            {
                { "javascript", new[] { "js" } }, { "js", new[] { "javascript" } },
                { "c#", new[] { "c sharp", "c-sharp" } }, { "c sharp", new[] { "c#", "c-sharp" } },
                { "react", new[] { "react.js", "reactjs" } }, { "sql", new[] { "t-sql", "mysql" } }
            };

            if (synonyms.ContainsKey(a) && synonyms[a].Contains(b)) return true;
            if (synonyms.ContainsKey(b) && synonyms[b].Contains(a)) return true;
            return false;
        }

        private int CalculateMatchScore(Person a, Person b)
        {
            int score = 0;
            // Jobbtitel ger 3 poäng
            if (!string.IsNullOrEmpty(a.JobTitle) && a.JobTitle.Equals(b.JobTitle, StringComparison.OrdinalIgnoreCase)) score += 3;

            // Skills ger 2 poäng per match
            if (!string.IsNullOrEmpty(a.Skills) && !string.IsNullOrEmpty(b.Skills))
            {
                var skillsA = a.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower());
                var skillsB = b.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower());
                foreach (var skA in skillsA)
                {
                    foreach (var skB in skillsB)
                    {
                        if (FuzzySkillMatch(skA, skB)) { score += 2; break; }
                    }
                }
            }

            // Utbildning ger 2 poäng (Tillägg för att matcha PersonController)
            if (!string.IsNullOrEmpty(a.Education) && a.Education.Equals(b.Education, StringComparison.OrdinalIgnoreCase)) score += 2;

            return score;
        }

        // --- ÖVRIGA METODER ---

        public async Task<IActionResult> Search(string search, string skill)
        {
            var query = _context.Persons.Where(p => p.IsActive);

            if (User.Identity?.IsAuthenticated != true)
                query = query.Where(p => !p.IsPrivate);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.FirstName.Contains(search) || p.LastName.Contains(search));
            }

            if (!string.IsNullOrEmpty(skill))
            {
                query = query.Where(p => p.Skills.Contains(skill) || p.Education.Contains(skill) || p.JobTitle.Contains(skill));
            }

            ViewBag.personResult = await query.ToListAsync();

            var projektResult = await _context.Projects
                .Where(p => string.IsNullOrEmpty(search) || p.ProjectName.Contains(search))
                .ToListAsync();

            return View("SearchResult", projektResult);
        }

        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}