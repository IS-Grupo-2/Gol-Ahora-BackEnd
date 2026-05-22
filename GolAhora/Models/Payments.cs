using System.Globalization;

namespace GolAhora.Models
{
    public class Payments
    {
        public int idPayment { get; set; }
        public int idClient { get; set; }
        public ClientProfile client { get; set; } = null!;
        public double amount { get; set; }
        public DateTime paymentDate { get; set; }
        public string paymentMethod { get; set; } = null!;
        public bool isSuccessful { get; set; }
        public int? idDiscount { get; set; }
        public Discounts? discount { get; set; }
        public Reservation reservation { get; set; } = null!;   
        public ICollection<CompetenceTeam> competenceTeams { get; set; } = new List<CompetenceTeam>();
    }

    public class Discounts
    {
        public int idDiscount { get; set; }
        public string nombre { get; set; } = null!;
        public string discountType { get; set; } = null!;
        public double discountValue { get; set; }
        public string conditions { get; set; } = null!;
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public Receipts receipt { get; set; } = null!;

        public ICollection<Payments> payments { get; set; } = new List<Payments>();
    }
}
