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

            ViewBag.SenasteProjekt = senasteProjekt;

            return View(urvalCV);
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Search(string search)
        {
            if (string.IsNullOrEmpty(search)){
                return RedirectToAction("Index");
            }
            string searchUpper = search.ToUpper();

            var projektResult = _context.Projects
            .Where(p => p.ProjectName.ToUpper().Contains(searchUpper))
            .ToList();

            var personResult = _context.Persons
                    .Where(p => p.FirstName.ToUpper().Contains(searchUpper) ||
                        p.LastName.ToUpper().Contains(searchUpper) ||
                        p.JobTitle.ToUpper().Contains(searchUpper))
                    .   ToList();

            ViewBag.SearchQuery = search;
            ViewBag.personResult = personResult;
            

            return View("SearchResult", projektResult);
        }
    }
}