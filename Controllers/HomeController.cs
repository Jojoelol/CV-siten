using CV_siten.Data;
using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models;
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
            // 1. Hämta de 3 senaste offentliga profilerna (Krav 1 & 6)
            var urvalCV = await _context.Persons
                .Where(p => p.AktivtKonto) // Endast aktiva och offentliga (Krav 6 & 12)
                .Take(3)
                .ToListAsync();

            // 2. Hämta det absolut senaste projektet (Krav 1)
            var senasteProjekt = await _context.Projekt
                .OrderByDescending(p => p.Startdatum) // Eller ID om ni inte har skapat-datum
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