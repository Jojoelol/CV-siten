using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels.Account // Tips: Se till att namespacet matchar mappen
{
    public class EditAccountViewModel
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt.")]
        [StringLength(50, ErrorMessage = "Förnamnet får inte överskrida 50 tecken.")]
        [Display(Name = "Förnamn")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Efternamn är obligatoriskt.")]
        [StringLength(50, ErrorMessage = "Efternamnet får inte överskrida 50 tecken.")]
        [Display(Name = "Efternamn")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-postadress är obligatorisk.")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress.")]
        [Display(Name = "E-post")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefonnummer är obligatoriskt.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Telefonnummer får endast innehålla siffror.")]
        [Display(Name = "Telefonnummer")]
        public string PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Yrkestiteln får inte överskrida 100 tecken.")]
        [Display(Name = "Yrkestitel")]
        public string JobTitle { get; set; }

        [StringLength(500, ErrorMessage = "Beskrivningen får inte överskrida 500 tecken.")]
        [Display(Name = "Om mig / Beskrivning")]
        public string Description { get; set; }
    }
}