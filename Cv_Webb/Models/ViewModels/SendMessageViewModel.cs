using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels
{
    public class SendMessageViewModel
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required(ErrorMessage = "Du måste skriva ett meddelande.")]
        [StringLength(2000, ErrorMessage = "Meddelandet får inte vara längre än 2000 tecken.")]
        [Display(Name = "Meddelande")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du måste skriva ett ämne.")]
        [StringLength(120, ErrorMessage = "Ämnet får inte vara längre än 120 tecken")]
        public string Subject { get; set; } = string.Empty;
    }
}