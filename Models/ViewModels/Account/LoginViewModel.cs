using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-postadress krävs")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        [Display(Name = "E-post")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lösenord krävs")]
        [DataType(DataType.Password)]
        [Display(Name = "Lösenord")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Kom ihåg mig")]
        public bool RememberMe { get; set; }
    }
}