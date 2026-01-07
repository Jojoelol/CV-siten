using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels.Message;
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
                .Where(m => m.ReceiverId == me.Id)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            return View("~/Views/Account/Message.cshtml", messages);
        }

        [HttpGet]
        public IActionResult Send(int receiverId) 
        {
            return View(new SendMessageViewModel { ReceiverId = receiverId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendMessageViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var me = await GetMyPersonAsync();

            if (vm.ReceiverId == me.Id)
            {
                ModelState.AddModelError("", "Du kan inte skicka meddelande till dig själv.");
                return View(vm);
            }

            var receiverExists = await _db.Persons.AnyAsync(p => p.Id == vm.ReceiverId);
            if (!receiverExists)
            {
                ModelState.AddModelError("", "Mottagaren finns inte.");
                return View(vm);
            }

            var entity = new Message
            {
                SenderId = me.Id,
                ReceiverId = vm.ReceiverId,
                Content = vm.Content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
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

            var msg = await _db.Messages.FirstOrDefaultAsync(m => m.Id == id && m.ReceiverId == me.Id);
            if (msg == null) return NotFound();

            msg.IsRead = true;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Inbox));
        }
    }
}