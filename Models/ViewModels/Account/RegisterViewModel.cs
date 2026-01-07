using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "E-postadress krävs")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        [Display(Name = "E-post")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lösenord krävs")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenordet måste vara minst 6 tecken långt.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lösenord")]
        // 1. Ändrat från Losenord till Password
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Bekräfta lösenord")]
        // 2. Ändrat Compare-referensen till "Password"
        [Compare("Password", ErrorMessage = "Lösenorden matchar inte.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}