namespace CV_siten.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Fornamn { get; set; }
        public string Efternamn { get; set; }
        public string Email { get; set; }
        public int Telefonnummer { get; set; }
        public string BildUrl { get; set; }
        public string Losenord { get; set; }
        public string Beskrivning { get; set; }
        public string Yrkestitel { get; set; }
        public bool Aktivtkonto { get; set; }
    }
}
