using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels.Account // Tips: Flytta denna till Webb-projektet senare
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt")]
        [Display(Name = "Förnamn")] // Detta gör att användaren fortfarande ser "Förnamn" i webbläsaren
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Efternamn är obligatoriskt")]
        [Display(Name = "Efternamn")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-post är obligatorisk")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        [Display(Name = "E-post")]
        public string Email { get; set; }

        [Display(Name = "Telefonnummer")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Lösenordet måste vara minst 3 tecken.")]
        [Display(Name = "Lösenord")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Du måste bekräfta lösenordet")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Lösenorden matchar inte.")] // Måste matcha nya namnet 'Password'
        [Display(Name = "Bekräfta lösenord")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Beskrivning")]
        public string? Description { get; set; }

        [Display(Name = "Yrkestitel")]
        public string? JobTitle { get; set; }
    }
}