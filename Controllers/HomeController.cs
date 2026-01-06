using System.Diagnostics;
using CV_siten.Models;
using CV_siten.Data;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Index()
        {
            // Hämta profilen till Model
            var profil = _context.Persons.FirstOrDefault();

            // Hämta senaste projektet till ViewBag
            ViewBag.SenasteProjekt = _context.Projekt
                                             .OrderByDescending(p => p.Id)
                                             .FirstOrDefault();

            return View(profil); // Skickar profil till @model
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

            var projektResult = _context.Projekt
            .Where(p => p.Projektnamn.ToUpper().Contains(searchUpper))
            .ToList();

            var personResult = _context.Persons
                    .Where(p => p.Fornamn.ToUpper().Contains(searchUpper) ||
                        p.Efternamn.ToUpper().Contains(searchUpper) ||
                        p.Yrkestitel.ToUpper().Contains(searchUpper))
                    .   ToList();

            ViewBag.SearchQuery = search;
            ViewBag.personResult = personResult;
            

            return View("SearchResult", projektResult);
        }
    }
}