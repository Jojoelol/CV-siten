using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CV_siten.Models.ViewModels // Kontrollera att detta matchar din mappstruktur
{
    public class EditProjectViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Projektnamn måste anges")]
        [Display(Name = "Projektnamn")]
        public string ProjectName { get; set; } = string.Empty;

        [Display(Name = "Beskrivning")]
        public string? Description { get; set; }

        [Display(Name = "Startdatum")]
        public DateTimeOffset StartDate { get; set; }

        [Display(Name = "Slutdatum")]
        public DateTimeOffset? EndDate { get; set; }

        public string? Type { get; set; }
        public string? Status { get; set; }

        // För att behålla befintliga filer
        public string? ImageUrl { get; set; }
        public string? ZipUrl { get; set; }

        // För nya uppladdningar
        public IFormFile? ImageFile { get; set; }
        public IFormFile? ZipFile { get; set; }
    }
}