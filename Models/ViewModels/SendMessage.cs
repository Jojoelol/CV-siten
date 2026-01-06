using System.ComponentModel.DataAnnotations;

namespace CV_siten.Models.ViewModels
{
    public class SendMessage
    {
        [Required]
        public int MottagareId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Innehall { get; set; } = string.Empty;
    }
}
