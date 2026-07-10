namespace GolAhora.Models
{
    public class Reservation
    {
        public int idReservation { get; set; }
        public int idClient { get; set; }
        public ClientProfile client { get; set; } = null!;
        public int idCourt { get; set; }
        public Court court { get; set; } = null!;
        public DateTime reservationDate { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public string status { get; set; } = "pendiente";
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public bool isPaid { get; set; }
        public double totalPrice { get; set; }
        public int idPayment { get; set; }
        public Payments payment { get; set; } = null!;
    }
}
