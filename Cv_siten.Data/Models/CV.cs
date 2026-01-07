using System.ComponentModel.DataAnnotations;

namespace CV_siten.Data.Models
{
    public class CV
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Filnamn")]
        public string FileName { get; set; } = string.Empty;

        [Display(Name = "Uppladdningsdatum")]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        public int PersonId { get; set; }
        public virtual Person Person { get; set; } = null!;
    }
}