using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CV_siten.Models.ViewModels.Account
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt")]
        [MinLength(2, ErrorMessage = "Förnamn måste vara minst två bokstäver.")]
        [RegularExpression(@"^[A-Öa-ö\s]+$", ErrorMessage = "Förnamn får endast innehålla bokstäver.")]
        [Display(Name = "Förnamn")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Efternamn är obligatoriskt")]
        [MinLength(2, ErrorMessage = "Efternamn måste vara minst två bokstäver.")]
        [RegularExpression(@"^[A-Öa-ö\s]+$", ErrorMessage = "Efternamn får endast innehålla bokstäver.")]
        [Display(Name = "Efternamn")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-post är obligatorisk")]
        [RegularExpression(@"^[A-Za-z0-9]{3,}@[A-Za-z]{3,}\.(se|com|net)$",
            ErrorMessage = "E-post måste ha minst tre bokstäver före och efter @ och sluta med .se eller .com.")]
        [Display(Name = "E-post")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefonnummer är obligatoriskt")]
        [Phone(ErrorMessage = "Ogiltigt telefonnummer.")]
        [Display(Name = "Telefonnummer")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenordet måste vara minst 6 tecken.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$",
            ErrorMessage = "Lösenordet måste innehålla minst en versal (A–Z), en siffra (0–9) och ett specialtecken.")]
        [Display(Name = "Lösenord")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du måste bekräfta lösenordet")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Lösenorden matchar inte.")]
        [Display(Name = "Bekräfta lösenord")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "En beskrivning av dig själv är obligatorisk")]
        [MaxLength(300, ErrorMessage = "Beskrivningen får vara max 300 tecken.")]
        [Display(Name = "Beskrivning")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yrkestitel är obligatorisk")]
        [MinLength(2, ErrorMessage = "Yrkestitel måste vara minst två tecken.")]
        [Display(Name = "Yrkestitel")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du måste välja om profilen ska vara privat eller offentlig")]
        [Display(Name = "Privat profil")]
        public bool IsPrivate { get; set; }

        [Required(ErrorMessage = "Adress är obligatorisk")]
        [Display(Name = "Adress")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postnummer är obligatoriskt")]
        [RegularExpression(@"^\d{3}\s?\d{2}$", ErrorMessage = "Ange ett giltigt postnummer (t.ex. 123 45)")]
        [Display(Name = "Postnummer")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ort är obligatorisk")]
        [Display(Name = "Ort")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du måste ladda upp en profilbild")]
        [Display(Name = "Profilbild")]
        public IFormFile ImageFile { get; set; }
    }
}