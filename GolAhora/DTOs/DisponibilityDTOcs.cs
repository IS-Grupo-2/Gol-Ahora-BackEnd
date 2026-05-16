// DisponibilityDTO.cs
namespace GolAhora.DTOs
{
    public class DisponibilityDTO
    {
        public DayOfWeek day { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public bool isAvailable { get; set; }
        public int courtId { get; set; }
    }
}