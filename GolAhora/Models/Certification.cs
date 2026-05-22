namespace GolAhora.Models
{
    public class Certification
    {
        public int idCertification { get; set; }
        public int professorId { get; set; }
        public ProfessorProfile professor { get; set; } = null!;
        public string name { get; set; } = null!;
        public string institution { get; set; } = null!;
        public DateTime dateObtained { get; set; }
        public string numberCertificate { get; set; } = null!;
        public bool verified { get; set; }
        public string verifiedBy { get; set; } = null!;
    }
}
