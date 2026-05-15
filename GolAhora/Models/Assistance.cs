namespace GolAhora.Models
{
    public class Assistance
    {
        public int idAssistance { get; set; }
        public int clientId { get; set; }
        public Client client { get; set; } = null!;
        public int classId { get; set; }
        public Class clas { get; set; } = null!;
        public bool isAssisted { get; set; }
        public string observations { get; set; } = null!;
    }
}
