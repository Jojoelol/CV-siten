namespace CV_siten.Models
{
    public class PersonProjekt
    {
        // Främmande nycklar
        public int PersonId { get; set; }
        public Person Person { get; set; }

        public int ProjektId { get; set; }
        public Projekt Projekt { get; set; }

        // Den extra informationen som diagrammet kräver
        public string Roll { get; set; }
    }
}