namespace CV_siten.Data.Models
{
    public class PersonProject
    {
        public int PersonId { get; set; }
        public virtual Person Person { get; set; } = null!;

        public int ProjectId { get; set; }

        public virtual Project Project { get; set; } = null!;

        public string? Role { get; set; }
    }
}