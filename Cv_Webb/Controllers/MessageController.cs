using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Controllers
{
    [AllowAnonymous]
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

        private async Task<Person?> TryGetMyPersonAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _db.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            var me = await TryGetMyPersonAsync();

            if (me == null)
            {
                return View("~/Views/Account/Message.cshtml", new List<CV_siten.Data.Models.Message>());
            }

            var messages = await _db.Messages
                .Where(m => m.ReceiverId == me.Id)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            return View("~/Views/Account/Message.cshtml", messages);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendMessageViewModel vm)
        {
            if (!ModelState.IsValid)
                return await ReturnInboxWithModalAsync(vm);

            var me = await TryGetMyPersonAsync();

            // Om inloggad: blocka "skicka till dig själv"
            if (me != null && vm.ReceiverId == me.Id)
            {
                ModelState.AddModelError("ReceiverId", "Du kan inte skicka meddelande till dig själv.");
                return await ReturnInboxWithModalAsync(vm);
            }

            var receiverExists = await _db.Persons.AnyAsync(p => p.Id == vm.ReceiverId);
            if (!receiverExists)
            {
                ModelState.AddModelError("ReceiverId", "Mottagaren finns inte.");
                return await ReturnInboxWithModalAsync(vm);
            }

            var entity = new CV_siten.Data.Models.Message
            {
                SenderId = me?.Id,                      // OK nu när SenderId är int?
                SenderName = me == null ? vm.SenderName : null,
                SenderEmail = me == null ? vm.SenderEmail : null,

                ReceiverId = vm.ReceiverId,
                Subject = vm.Subject,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Message/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var me = await GetMyPersonAsync();

            var msg = await _db.Messages
                .FirstOrDefaultAsync(m => m.Id == id && m.ReceiverId == me.Id);

            if (msg == null)
                return NotFound();

            _db.Messages.Remove(msg);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Inbox));
        }

        private async Task<IActionResult> ReturnInboxWithModalAsync(SendMessageViewModel? vm = null)
        {
            var me = await GetMyPersonAsync();

            var messages = await _db.Messages
                .Where(m => m.ReceiverId == me.Id)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            if (vm != null)
            {
                ViewData["OpenSendModal"] = true;
                ViewData["SendReceiverId"] = vm.ReceiverId;
                ViewData["SendSubject"] = vm.Subject;
                ViewData["SendContent"] = vm.Content;
            }

            return View("~/Views/Account/Message.cshtml", messages);
        }
        [HttpGet]
        public async Task<IActionResult> SearchPerson(string q)
        {
            q = (q ?? "").Trim();
            if (q.Length < 2)
                return Json(Array.Empty<object>());

            var results = await _db.Persons
                .Where(p =>
                    (p.FirstName + " " + p.LastName).Contains(q) ||
                    p.FirstName.Contains(q) ||
                    p.LastName.Contains(q))
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.FirstName + " " + p.LastName,
                    imageUrl = string.IsNullOrWhiteSpace(p.ImageUrl)
                    ? Url.Content("~/images/ProfilePicture/defaultPicture.jpg")
                    : (p.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? p.ImageUrl
                         : p.ImageUrl.StartsWith("/") ? p.ImageUrl
                         : Url.Content("~/images/ProfilePicture/" + p.ImageUrl))
                })
                .Take(8)
                .ToListAsync();

            return Json(results);
        }

    }
}