using CV_siten.Data.Data;
using CV_siten.Data.Models;
using CV_siten.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_siten.Controllers
{
    [Authorize]
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PersonController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        //// --- VISA PROFIL ---
        //[AllowAnonymous]
        //public async Task<IActionResult> Profile(int? id, string searchString, string sortBy)
        //{
        //    Person person;
        //    if (id.HasValue)
        //    {
        //        person = await _context.Persons
        //            .Include(p => p.PersonProjects).ThenInclude(pp => pp.Project)
        //            .FirstOrDefaultAsync(p => p.Id == id);
        //    }
        //    else
        //    {
        //        var user = await _userManager.GetUserAsync(User);
        //        if (user == null) return RedirectToAction("Login", "Account");
        //        person = await _context.Persons
        //            .Include(p => p.PersonProjects).ThenInclude(pp => pp.Project)
        //            .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
        //    }

        //    if (person == null) return NotFound();

        //    // Sökning
        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        person.PersonProjects = person.PersonProjects
        //            .Where(pp => pp.Project.ProjectName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        //    }

        //    // Sortering
        //    person.PersonProjects = sortBy switch
        //    {
        //        "status" => person.PersonProjects.OrderBy(pp => pp.Project.Status).ToList(),
        //        "tid" => person.PersonProjects.OrderByDescending(pp => pp.Project.StartDate).ToList(),
        //        _ => person.PersonProjects.OrderBy(pp => pp.Project.ProjectName).ToList()
        //    };

        //    ViewBag.CurrentSearch = searchString;
        //    return View(person); 
        //}

        // --- VISA PROFIL ---
        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? id, string searchString, string sortBy)
        {
            // 1. Hämta den inloggade användaren (om någon)
            var user = await _userManager.GetUserAsync(User);
            var loggedInPerson = user != null
                ? await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id)
                : null;

            // 2. Bestäm vilken person som ska visas
            // Om id saknas i URL, visa den inloggades profil. Om inte inloggad, gå till login.
            if (!id.HasValue && loggedInPerson == null) return RedirectToAction("Login", "Account");
            int targetId = id ?? loggedInPerson.Id;

            // 3. Kolla om besökaren är ägaren (för att visa/dölja knappar)
            ViewBag.IsOwner = (loggedInPerson != null && targetId == loggedInPerson.Id);

            // 4. Hämta personen från databasen
            var person = await _context.Persons
                .Include(p => p.PersonProjects).ThenInclude(pp => pp.Project)
                .FirstOrDefaultAsync(p => p.Id == targetId);

            if (person == null) return NotFound();

            //[cite_start]// 5. Krav 7: Hantera privat profil 
                        // Om profilen är privat och besökaren INTE är inloggad -> skicka till login
            //if (person.IsPrivate && user == null)
            //{
            //    return RedirectToAction("Login", "Account");
           // }

            // --- Sökning och sortering (din befintliga kod) ---
            if (!string.IsNullOrEmpty(searchString))
            {
                person.PersonProjects = person.PersonProjects
                    .Where(pp => pp.Project.ProjectName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            person.PersonProjects = sortBy switch
            {
                "status" => person.PersonProjects.OrderBy(pp => pp.Project.Status).ToList(),
                "tid" => person.PersonProjects.OrderByDescending(pp => pp.Project.StartDate).ToList(),
                _ => person.PersonProjects.OrderBy(pp => pp.Project.ProjectName).ToList()
            };

            ViewBag.CurrentSearch = searchString;

            // Om du flyttade filen till Views/Account måste du skriva hela sökvägen:
            // return View("~/Views/Account/Profile.cshtml", person);
            return View(person);
        }

        // --- REDIGERA PROFIL ---
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null) return NotFound();

            var model = new EditAccountViewModel
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = user.Email,
                PhoneNumber = person.PhoneNumber,
                JobTitle = person.JobTitle,
                Description = person.Description
            };
            return View(model); 
        }

        //INAKTIVERA PROFIL 
        [HttpPost]
        public async Task<IActionResult> Inactivate(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null)
                return NotFound();

            person.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", new { id = person.Id });
        }
        //AKTIVERA PROFIL
        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null)
                return NotFound();

            person.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", new { id = person.Id });
        }


        // --- REDIGERA KOMPETENSER, UTBILDNINGAR OCH TIDIGARE ERFARENHETER ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfileField(int id, string fieldName, string fieldValue)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null) return NotFound();

            // Kontrollera att det är ägaren som försöker spara
            var user = await _userManager.GetUserAsync(User);
            if (person.IdentityUserId != user.Id) return Forbid();

            // Uppdatera rätt fält baserat på namnet vi skickar in
            switch (fieldName)
            {
                case "Skills": person.Skills = fieldValue; break;
                case "Education": person.Education = fieldValue; break;
                case "Experience": person.Experience = fieldValue; break;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", new { id = person.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (person == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            await _userManager.UpdateAsync(user);

            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            person.PhoneNumber = model.PhoneNumber;
            person.JobTitle = model.JobTitle;
            person.Description = model.Description;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profilen har uppdaterats!";
            return RedirectToAction(nameof(Profile));
        }

        // --- CV-HANTERING ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCv(IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0) return RedirectToAction(nameof(Profile));

            var extension = Path.GetExtension(cvFile.FileName).ToLower();
            if (extension != ".pdf") return BadRequest("Endast PDF tillåtet.");

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null)
            {
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(person.CvUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, person.CvUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                person.CvUrl = "/uploads/cvs/" + fileName;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCv()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person != null && !string.IsNullOrEmpty(person.CvUrl))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, person.CvUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                person.CvUrl = null;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Profile));
        }
    }
}