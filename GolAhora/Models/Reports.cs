namespace GolAhora.Models
{
    public class Reports
    {
        public int idReport { get; set; }
        public string tittle { get; set; } = null!;
        public string type { get; set; } = null!;
        public DateTime generatedDate { get; set; }
        public string content { get; set; } = null!;
        public int idAdmin { get; set; }
        public Admin admin { get; set; } = null!;
        public DateTime from { get; set; }
        public DateTime to { get; set; }
    }
}
