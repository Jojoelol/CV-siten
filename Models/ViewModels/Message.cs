using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_siten.Models.ViewModels
{
    [Table("Meddelanden")]
    public class Message
    {
        public int Id { get; set; }

        public int AvsandareId { get; set; }
        public int MottagareId { get; set; }

        public string Innehall { get; set; } = string.Empty;
        public DateTime Tidsstampel { get; set; }
        public bool ArLast { get; set; }

        public Person Avsandare { get; set; } = null!;
        public Person Mottagare { get; set; } = null!;
    }
}