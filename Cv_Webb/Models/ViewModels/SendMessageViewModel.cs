using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels
{
    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "Välj en mottagare.")]
        public int? ReceiverId { get; set; }

        [Required(ErrorMessage = "Du måste skriva ett meddelande.")]
        [StringLength(200, ErrorMessage = "Meddelandet får inte vara längre än 200 tecken.")]
        [Display(Name = "Meddelande")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du måste skriva ett ämne.")]
        [StringLength(50, ErrorMessage = "Ämnet får inte vara längre än 50 tecken")]
        public string Subject { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }

    }
}