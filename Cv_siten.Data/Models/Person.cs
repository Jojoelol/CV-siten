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
        
        [Display(Name = "Färdigheter")]
        public string? Skills { get; set; }

        [Display(Name = "Utbildning")]
        public string? Education { get; set; }

        [Display(Name = "Erfarenheter")]
        public string? Experience { get; set; }

        [Display(Name = "Adress")]
        public string? Address { get; set; }

        [Display(Name = "Postnummer")]
        public string? PostalCode { get; set; }

        [Display(Name = "Ort")]
        public string? City { get; set; }

        [Display(Name = "Aktivt konto")]
        public bool IsActive { get; set; }

        [Display(Name = "CV-sökväg")]
        public string? CvUrl { get; set; }

        [Display(Name = "Antal besök")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "Privat profil")]
        public bool IsPrivate { get; set; } = false; 

        public virtual ICollection<PersonProject> PersonProjects { get; set; } = new List<PersonProject>();

        public string IdentityUserId { get; set; } = string.Empty;
        public virtual ApplicationUser IdentityUser { get; set; } = null!;

    }
}