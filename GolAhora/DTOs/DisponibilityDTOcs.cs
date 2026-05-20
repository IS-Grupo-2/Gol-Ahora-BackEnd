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

    public class DisponibilityResponseDTO
    {
        public int idDisponibility { get; set; }
        public DayOfWeek day { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public bool isAvailable { get; set; }
        public string courtName { get; set; } = null!;
    }

    public class DisponibilityDetailDTO
    {
        public int idDisponibility { get; set; }
        public DayOfWeek day { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public bool isAvailable { get; set; }
        public int courtId { get; set; }
        public string courtName { get; set; } = null!;
    }
}