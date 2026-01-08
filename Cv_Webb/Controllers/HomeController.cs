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
            ViewBag.SenasteProjekt = await _context.Projects
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefaultAsync();

            // Om utloggad â visa 3 utvalda profiler
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.SectionTitle = "UTVALDA PROFILER";
                ViewBag.MatchData = new Dictionary<int, int>(); // viktigt!

                var urvalCV = await _context.Persons
                    .Where(p => p.IsActive && !p.IsPrivate)
                    .Take(3)
                    .ToListAsync();

                return View(urvalCV);
            }

            // HÃ¤mta inloggad anvÃ¤ndare
            var userId = _userManager.GetUserId(User);
            var currentPerson = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Om ingen Person-profil finns â visa utvalda
            if (currentPerson == null)
            {
                ViewBag.SectionTitle = "UTVALDA PROFILER";
                ViewBag.MatchData = new Dictionary<int, int>();

                var fallback = await _context.Persons
                    .Where(p => p.IsActive && !p.IsPrivate)
                    .Take(3)
                    .ToListAsync();

                return View(fallback);
            }

            // HÃ¤mta andra personer
            var others = await _context.Persons
                .Where(p => p.Id != currentPerson.Id && p.IsActive && !p.IsPrivate)
                .ToListAsync();



         // MATCHANDE AV PROFILER
            var matches = others
                .Select(p =>
                {
                    var score = CalculateMatchScore(currentPerson, p);
                    return new
                    {
                        Person = p,
                        Score = score,
                        MatchPercent = score * 10 // maxscore 10 â 100%
                    };
                })
                .Where(x => x.Score >= 5) // minst 50%
                .OrderByDescending(x => x.Score)
                .Take(3)
                .ToList();

            // Skicka procent till vyn
            ViewBag.MatchData = matches.ToDictionary(
                x => x.Person.Id,
                x => x.MatchPercent
            );

            ViewBag.SectionTitle = "LIKNANDE PROFILER";

            return View(matches.Select(x => x.Person).ToList());
        }

        // BERÃKNAR MATCHNINGSSCORE MELLAN TVÃ PERSONER
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
    }
}
