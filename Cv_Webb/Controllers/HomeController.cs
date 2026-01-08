using CV_siten.Data;
using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace CV_siten.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Senaste projektet
            var senasteProjekt = await _context.Projects
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefaultAsync();

            ViewBag.SenasteProjekt = senasteProjekt;

            // Om ingen är inloggad → visa INGA profiler
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.SectionTitle = "";
                return View(new List<Person>()); // tom lista
            }

            // Hämta inloggad användare
            var userId = _userManager.GetUserId(User);
            var currentPerson = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Om något saknas → visa INGA profiler
            if (currentPerson == null)
            {
                ViewBag.SectionTitle = "";
                return View(new List<Person>());
            }

            // Hämta andra personer
            var others = await _context.Persons
                .Where(p => p.Id != currentPerson.Id && p.IsActive)
                .ToListAsync();

            // Matchning + procent + 50%-gräns
            var matches = others
                .Select(p =>
                {
                    var score = CalculateMatchScore(currentPerson, p);
                    return new
                    {
                        Person = p,
                        Score = score,
                        MatchPercent = score * 10 // maxscore 10 → 100%
                    };
                })
                .Where(x => x.Score >= 5) // minst 50%
                .OrderByDescending(x => x.Score)
                .Take(3)
                .ToList();

            // Skicka matchningsprocent till vyn
            ViewBag.MatchData = matches.ToDictionary(
                x => x.Person.Id,
                x => x.MatchPercent
            );

            ViewBag.SectionTitle = "LIKNANDE PROFILER";

            return View(matches.Select(x => x.Person).ToList());
        }

        // Matchningslogik
        private int CalculateMatchScore(Person a, Person b)
        {
            int score = 0;

            if (!string.IsNullOrEmpty(a.JobTitle) &&
                a.JobTitle.Equals(b.JobTitle, StringComparison.OrdinalIgnoreCase))
                score += 3;

            if (!string.IsNullOrEmpty(a.Skills) &&
                !string.IsNullOrEmpty(b.Skills))
            {
                var skillsA = a.Skills.Split(',').Select(s => s.Trim());
                var skillsB = b.Skills.Split(',').Select(s => s.Trim());

                if (skillsA.Intersect(skillsB).Any())
                    score += 5;
            }

            if (!string.IsNullOrEmpty(a.Education) &&
                a.Education.Equals(b.Education, StringComparison.OrdinalIgnoreCase))
                score += 2;

            return score;
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public async Task<IActionResult> Search(string search, string skill)
        {
            var query = _context.Persons.AsQueryable();

            query = query.Where(p => p.IsActive);

            if (!User.Identity.IsAuthenticated)
            {
                query = query.Where(p => !p.IsPrivate);
            }

            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToUpper();
                query = query.Where(p =>
                    p.FirstName.ToUpper().Contains(s) ||
                    p.LastName.ToUpper().Contains(s));
            }

            if (!string.IsNullOrEmpty(skill))
            {
                string sk = skill.ToUpper();
                query = query.Where(p =>
                    (p.Skills != null && p.Skills.ToUpper().Contains(sk)) ||
                    (p.Education != null && p.Education.ToUpper().Contains(sk)) ||
                    (p.JobTitle != null && p.JobTitle.ToUpper().Contains(sk)) ||
                    (p.Experience != null && p.Experience.ToUpper().Contains(sk))
                );
            }

            var personResult = await query.ToListAsync();

            var projektResult = await _context.Projects
                .Where(p => string.IsNullOrEmpty(search) ||
                            p.ProjectName.ToUpper().Contains(search.ToUpper()))
                .ToListAsync();

            ViewBag.SearchQuery = search;
            ViewBag.SkillQuery = skill;
            ViewBag.personResult = personResult;

            return View("SearchResult", projektResult);
        }
    }
}
