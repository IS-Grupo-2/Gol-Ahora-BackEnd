namespace GolAhora.Models
{
    public class Class
    {
        public int idClass { get; set; }
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public string classType { get; set; } = null!;
        public int profesorId { get; set; }
        public ProfessorProfile profesor { get; set; } = null!;
        public int courtId { get; set; }
        public Court court { get; set; } = null!;
        public DateTime date { get; set; }
        public int capacityMax { get; set; }
        public int duration { get; set; }
        public double price { get; set; }
        public bool isActive { get; set; }
        public ICollection<ClientProfile> clients { get; set; } = new List<ClientProfile>();
        public ICollection<Assistance> assistances { get; set; } = new List<Assistance>();
    }
}
