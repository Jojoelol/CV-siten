namespace CV_siten.Models
{
    public class Projekt
    {
        public int Id { get; set; }
        public string Projektnamn { get; set; }
        public string Beskrivning { get; set; }
        public DateTimeOffset Startdatum { get; set; }
        public DateTimeOffset? Slutdatum { get; set; }
        // Relation: Ett projekt kan ha många personer
        public virtual ICollection<PersonProjekt> PersonProjekt { get; set; } = new List<PersonProjekt>();
        public string Typ { get; set; }
        public string Status { get; set; }

       // public string Fil { get; set; }
    }
}
