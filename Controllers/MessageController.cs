using CV_siten.Data;
using CV_siten.Models;
using CV_siten.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager; 

        public MessageController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<Person> GetMyPersonAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) throw new Exception("Inte inloggad.");

            var person = await _db.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null) throw new Exception("Ingen Person kopplad till kontot.");

            return person;
        }
        
        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            var me = await GetMyPersonAsync();

            var messages = await _db.Messages
                .Where(m => m.MottagareId == me.Id)
                .Include(m => m.Avsandare)
                .OrderByDescending(m => m.Tidsstampel)
                .ToListAsync();

            return View("~/Views/Account/Message.cshtml", messages);
        }

        // Form för att skicka (t.ex. från en profil)
        [HttpGet]
        public IActionResult Send(int mottagareId)
        {
            return View(new SendMessage { MottagareId = mottagareId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendMessage vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var me = await GetMyPersonAsync();

            if (vm.MottagareId == me.Id)
            {
                ModelState.AddModelError("", "Du kan inte skicka meddelande till dig själv.");
                return View(vm);
            }

            var mottagareFinns = await _db.Persons.AnyAsync(p => p.Id == vm.MottagareId);
            if (!mottagareFinns)
            {
                ModelState.AddModelError("", "Mottagaren finns inte.");
                return View(vm);
            }

            var entity = new Message
            {
                AvsandareId = me.Id,
                MottagareId = vm.MottagareId,
                Innehall = vm.Innehall,
                Tidsstampel = DateTime.UtcNow,
                ArLast = false
            };

            _db.Messages.Add(entity);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var me = await GetMyPersonAsync();

            var msg = await _db.Messages.FirstOrDefaultAsync(m => m.Id == id && m.MottagareId == me.Id);
            if (msg == null) return NotFound();

            msg.ArLast = true;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Message()
        {
            // Berätta specifikt för controllern att den ska titta i Account-mappen
            return View("~/Views/Account/Message.cshtml");
        }
    }
}
