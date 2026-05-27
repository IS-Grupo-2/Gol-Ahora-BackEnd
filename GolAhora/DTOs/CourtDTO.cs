namespace GolAhora.DTOs
{
    public class CourtDTO
    {
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public int courtTypeId { get; set; }
    }
    public class CourtResponseDTO
    {
        public int idCourt { get; set; }
        public string name { get; set; } = null!;
        public bool isAvailable { get; set; }
        public string? description { get; set; }
        public string courtTypeName { get; set; } = null!;
    }
    public class CourtDetailDTO
    {
        public int idCourt { get; set; }
        public string name { get; set; } = null!;
        public bool isAvailable { get; set; }
        public string? description { get; set; }
        public string courtTypeName { get; set; } = null!;
        public List<DisponibilitySummaryDTO> disponibilities { get; set; } = new();
    }
    public class DisponibilitySummaryDTO
    {
        public int idDisponibility { get; set; }
        public DayOfWeek day { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public bool isAvailable { get; set; }
    }
}