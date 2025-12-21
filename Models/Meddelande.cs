using System.Runtime.CompilerServices;

namespace CV_siten.Models
{
    public class Meddelande
    {
        public int Id{ get; set; }
        public string Innehall { get; set; }
        public DateTimeOffset Tidsstampel { get; set; }
        public bool ArLast { get; set; }
    }
}
