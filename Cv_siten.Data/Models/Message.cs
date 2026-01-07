using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_siten.Data.Models 
{
    [Table("Messages")]
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;


        [ForeignKey("SenderId")]
        public virtual Person Sender { get; set; } = null!;

        [ForeignKey("ReceiverId")]
        public virtual Person Receiver { get; set; } = null!;
    }
}