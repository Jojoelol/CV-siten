using CV_siten.Data;
using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;



namespace CV_siten.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var urvalCV = await _context.Persons
                .Where(p => p.IsActive) 
                .Take(3)
                .ToListAsync();


            var senasteProjekt = await _context.Projects
                .OrderByDescending(p => p.StartDate) 
                .FirstOrDefaultAsync();

            var offentligaProfiler = await _context.Persons
    .Where(p => p.IsActive && !p.IsPrivate) // Visa bara de som är aktiva OCH inte privata
    .ToListAsync();

            ViewBag.SenasteProjekt = senasteProjekt;

            return View(urvalCV);
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Search(string search, string skill)
        {
            var query = _context.Persons.AsQueryable();

            // VG-krav 3: Visa endast aktiva konton
            query = query.Where(p => p.IsActive);

            // G-krav 12: Om användaren inte är inloggad, dölj privata profiler
            if (!User.Identity.IsAuthenticated)
            {
                query = query.Where(p => !p.IsPrivate);
            }

            // Fält 1 (name="search"): Sök endast på namn
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToUpper();
                query = query.Where(p =>
                    p.FirstName.ToUpper().Contains(s) ||
                    p.LastName.ToUpper().Contains(s));
            }

            // Fält 2 (name="skill"): Sök på Skills, Education, JobTitle och Experience
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

            // Sök efter projekt (valfritt om du vill att search-fältet även ska trigga projekt)
            var projektResult = await _context.Projects
                .Where(p => string.IsNullOrEmpty(search) || p.ProjectName.ToUpper().Contains(search.ToUpper()))
                .ToListAsync();

            // Skicka tillbaka värdena till vyn så de ligger kvar i rutorna efter sökning
            ViewBag.SearchQuery = search;
            ViewBag.SkillQuery = skill;
            ViewBag.personResult = personResult;

            return View("SearchResult", projektResult);
        }
    }
}