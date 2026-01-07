using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels.Account
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt")]
        [MinLength(2, ErrorMessage = "Förnamn måste vara minst två bokstäver.")]
        [RegularExpression(@"^[A-Öa-ö]+$", ErrorMessage = "Förnamn får endast innehålla bokstäver.")]
        [Display(Name = "Förnamn")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Efternamn är obligatoriskt")]
        [MinLength(2, ErrorMessage = "Efternamn måste vara minst två bokstäver.")]
        [RegularExpression(@"^[A-Öa-ö]+$", ErrorMessage = "Efternamn får endast innehålla bokstäver.")]
        [Display(Name = "Efternamn")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-post är obligatorisk")]
        [RegularExpression(@"^[A-Za-z]{3,}@[A-Za-z]{3,}\.(se|com)$",
            ErrorMessage = "E-post måste ha minst tre bokstäver före och efter @ och sluta med .se eller .com.")]
        [Display(Name = "E-post")]
        public string Email { get; set; }

        [Display(Name = "Telefonnummer")]
        [Phone(ErrorMessage = "Ogiltigt telefonnummer.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenordet måste vara minst 6 tecken.")]
        [Display(Name = "Lösenord")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Du måste bekräfta lösenordet")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Lösenorden matchar inte.")]
        [Display(Name = "Bekräfta lösenord")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Beskrivning")]
        [MinLength(10, ErrorMessage = "Beskrivningen måste vara minst 10 tecken.")]
        public string? Description { get; set; }

        [Display(Name = "Yrkestitel")]
        [MinLength(2, ErrorMessage = "Yrkestitel måste vara minst två tecken.")]
        public string? JobTitle { get; set; }
    }
}
