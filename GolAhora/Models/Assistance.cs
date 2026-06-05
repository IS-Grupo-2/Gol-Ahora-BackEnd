namespace GolAhora.Models
{
    public class Assistance
    {
        public int idAssistance { get; set; }
        public int clientId { get; set; }
        public ClientProfile client { get; set; } = null!;
        public int classId { get; set; }
        public Class clas { get; set; } = null!;
        public DateTime date { get; set; }
        public bool isAssisted { get; set; }
        public string observations { get; set; } = null!;
    }
}
