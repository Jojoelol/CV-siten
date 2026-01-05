namespace CV_siten.Models
{
    public class EditAccountViewModel
    {
        public string Fornamn { get; set; }
        public string Efternamn { get; set; }
        public string Email { get; set; }
        public int Telefonnummer { get; set; }
        public string Yrkestitel { get; set; }
        public string Beskrivning { get; set; }
        public string Losenord { get; set; }
        // Lägg till lösenord här om du vill att användaren ska kunna byta det
    }
}