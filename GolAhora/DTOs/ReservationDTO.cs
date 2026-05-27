namespace GolAhora.DTOs
{
    public class ReservationDTO
    {
        public int idClient { get; set; }
        public int idCourt { get; set; }
        public DateTime reservationDate { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public int? idPayment { get; set; }
    }
    public class ReservationResponseDTO
    {
        public int idReservation { get; set; }
        public int idClient { get; set; }
        public string clienteNombre { get; set; } = null!;
        public string clienteApellido { get; set; } = null!;
        public int idCourt { get; set; }
        public string canchaNombre { get; set; } = null!;
        public DateTime reservationDate { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public bool isPaid { get; set; }
        public double totalPrice { get; set; }
        public int idPayment { get; set; }
    }
}