using System.Diagnostics;
using CV_siten.Models;
using CV_siten.Data; // Viktigt: Lägg till denna för att hitta din databas
using Microsoft.AspNetCore.Mvc;

namespace CV_siten.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Lägg till denna rad

        // Uppdatera konstruktorn så den tar emot _context
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Nu kan controllern prata med SQL Server
        }

        public IActionResult Index()
        {
            // 1. Hämta dig själv (den första personen i tabellen)
            var profil = _context.Persons.FirstOrDefault();

            // 2. Hämta det senaste projektet och lägg i ViewBag
            ViewBag.SenasteProjekt = _context.Projekt
                                             .OrderByDescending(p => p.Id)
                                             .FirstOrDefault();

            // 3. Skicka profilen till vyn
            return View(profil);
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Vi letar i tabellen "Persons" efter en matchning
            var user = _context.Persons.FirstOrDefault(p => p.Email == model.Email && p.Losenord == model.Losenord);

            if (user != null)
            {
                // Om vi hittar användaren, skicka dem till startsidan (eller en "Admin"-sida senare)
                return RedirectToAction("Index");
            }

            // Om det inte matchar, visa ett felmeddelande
            ViewBag.Error = "Felaktig e-post eller lösenord.";
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}