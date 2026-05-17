namespace GolAhora.DTOs
{
    public class CourtTypeDTO
    {
        public string name { get; set; } = null!;
        public double superficie { get; set; }
        public int capacity { get; set; }
        public double pricePerHour { get; set; }
        public string description { get; set; } = null!;
    }
}