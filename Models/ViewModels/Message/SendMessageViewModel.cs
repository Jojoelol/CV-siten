using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels.Message
{
    public class SendMessageViewModel
    {
        [Required]
        // 1. MottagareId -> ReceiverId
        public int ReceiverId { get; set; }

        [Required(ErrorMessage = "Du måste skriva ett meddelande.")]
        [StringLength(2000, ErrorMessage = "Meddelandet får inte vara längre än 2000 tecken.")]
        [Display(Name = "Meddelande")]
        // 2. Innehall -> Content
        public string Content { get; set; } = string.Empty;
    }
}