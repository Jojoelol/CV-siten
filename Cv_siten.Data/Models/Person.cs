using System.ComponentModel.DataAnnotations;

namespace CV_siten.Data.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Display(Name = "Förnamn")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Efternamn")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Telefonnummer")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Profilbild")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Beskrivning")]
        public string? Description { get; set; }

        [Display(Name = "Yrkestitel")]
        public string? JobTitle { get; set; }

        [Display(Name = "Aktivt konto")]
        public bool IsActive { get; set; }

        [Display(Name = "CV-sökväg")]
        public string? CvUrl { get; set; }

        public virtual ICollection<PersonProject> PersonProjects { get; set; } = new List<PersonProject>();

        public string IdentityUserId { get; set; } = string.Empty;
        public virtual ApplicationUser IdentityUser { get; set; } = null!;
    }
}