namespace CV_siten.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Fornamn { get; set; }
        public string Efternamn { get; set; }
        public string Telefonnummer { get; set; }
        public string BildUrl { get; set; }
        public string Beskrivning { get; set; }
        public string Yrkestitel { get; set; }
        public bool AktivtKonto { get; set; }
        public virtual ICollection<PersonProjekt> PersonProjekt { get; set; } = new List<PersonProjekt>() { };

        public string IdentityUserId { get; set; }
        public ApplicationUser IdentityUser { get; set; }
    }
}
