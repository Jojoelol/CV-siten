namespace CV_siten.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Fornamn { get; set; } = string.Empty;
        public string Efternamn { get; set; } = string.Empty;

        // Ändrat till string för att hantera nollor i början och +46
        public string? Telefonnummer { get; set; }

        public string? BildUrl { get; set; }
        public string? Beskrivning { get; set; }
        public string? Yrkestitel { get; set; }
<<<<<<< HEAD
=======
        
>>>>>>> main
        public bool AktivtKonto { get; set; }

        // Bra! Här lagras sökvägen till CV-filen (PDF)
        public string? CvUrl { get; set; }

        // Navigation till kopplingstabellen för projekt
        public virtual ICollection<PersonProjekt> PersonProjekt { get; set; } = new List<PersonProjekt>();

        // Koppling till IdentityUser (Inloggning)
        public string IdentityUserId { get; set; } = string.Empty;
        public virtual ApplicationUser IdentityUser { get; set; } = null!;
    }
}