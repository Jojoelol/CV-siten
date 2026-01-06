using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_siten.Models
{
    public class Meddelande
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Innehall { get; set; }

        public DateTime Tidsstampel { get; set; } = DateTime.Now;

        // Krav 13 & 14: För att kunna markera som läst och visa notifikationer
        public bool ArLast { get; set; } = false;

        // Avsändarens namn (Krav 11: kan vara inloggad användare eller anonymt namn)
        [Required(ErrorMessage = "Avsändarnamn saknas")]
        public string AvsandareNamn { get; set; }

        // Relation till mottagaren (Personen vars CV man är inne på)
        public int MottagareId { get; set; }

        [ForeignKey("MottagareId")]
        public virtual Person Mottagare { get; set; }
    }
}