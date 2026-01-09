using System.ComponentModel.DataAnnotations;

namespace CV_siten.Data.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Projektnamn")]
        public string ProjectName { get; set; } = string.Empty;

        [Display(Name = "Beskrivning")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Startdatum")]
        public DateTimeOffset StartDate { get; set; }

        [Display(Name = "Slutdatum")]
        public DateTimeOffset? EndDate { get; set; }

        public virtual ICollection<PersonProject> PersonProjects { get; set; } = new List<PersonProject>();

        [Display(Name = "Typ")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Projektbild")]
        public string? ImageUrl { get; set; }

        // --- NY EGENSKAP FÖR ZIP-FILER ---
        [Display(Name = "Källkod (ZIP)")]
        public string? ZipUrl { get; set; }

        [Display(Name = "Projektägare")]
        public int OwnerId { get; set; }

        public virtual Person? Owner { get; set; }
    }
}