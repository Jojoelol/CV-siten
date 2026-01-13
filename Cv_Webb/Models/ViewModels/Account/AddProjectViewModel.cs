using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels
{
    public class AddProjectViewModel
    {
        [Required(ErrorMessage = "Projektnamn är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Projektnamnet får inte överskrida 100 tecken.")]
        [Display(Name = "Projektnamn")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Du måste ange din roll i projektet.")]
        [Display(Name = "Roll")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Du måste ange typ av projekt.")]
        [Display(Name = "Typ")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Status är obligatoriskt.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Startdatum är obligatoriskt.")]
        [DataType(DataType.Date)]
        [Display(Name = "Startdatum")]
        public DateTime? StartDate { get; set; } 

        [DataType(DataType.Date)]
        [Display(Name = "Slutdatum")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "En beskrivning av projektet krävs.")]
        [MaxLength(500, ErrorMessage = "Beskrivningen får vara max 500 tecken.")]
        [Display(Name = "Beskrivning")]
        public string Description { get; set; }


        // Fält för filuppladdning
        public IFormFile? ImageFile { get; set; }
        public IFormFile? ZipFile { get; set; }
    }
}