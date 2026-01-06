using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt")]
        public string Fornamn { get; set; }

        [Required(ErrorMessage = "Efternamn är obligatoriskt")]
        public string Efternamn { get; set; }

        [Required(ErrorMessage = "E-post är obligatorisk")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        public string Email { get; set; }

        public string Telefonnummer { get; set; }

        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Lösenordet måste vara minst 3 tecken.")]
        public string Losenord { get; set; }

        [Required(ErrorMessage = "Du måste bekräfta lösenordet")]
        [DataType(DataType.Password)]
        [Compare("Losenord", ErrorMessage = "Lösenorden matchar inte.")]
        public string BekraftaLosenord { get; set; }

        public string Beskrivning { get; set; }
        public string Yrkestitel { get; set; }
    }
}