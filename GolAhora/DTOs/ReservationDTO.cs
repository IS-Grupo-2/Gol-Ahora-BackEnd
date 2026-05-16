// ReservationDTO.cs
namespace GolAhora.DTOs
{
    public class ReservationDTO
    {
        public int idClient { get; set; }
        public int idCourt { get; set; }
        public DateTime reservationDate { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public double totalPrice { get; set; }
        public int? idPayment { get; set; }
    }
}