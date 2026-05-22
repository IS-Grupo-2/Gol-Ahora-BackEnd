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

    public class CourtTypeResponseDTO
    {
        public int idTypeCourt { get; set; }
        public string name { get; set; } = null!;
        public double superficie { get; set; }
        public int capacity { get; set; }
        public double pricePerHour { get; set; }
        public string description { get; set; } = null!;
    }

    public class CourtTypeDetailDTO
    {
        public int idTypeCourt { get; set; }
        public string name { get; set; } = null!;
        public double superficie { get; set; }
        public int capacity { get; set; }
        public double pricePerHour { get; set; }
        public string description { get; set; } = null!;
        public List<CourtSummaryDTO> courts { get; set; } = new();
    }

    public class CourtSummaryDTO
    {
        public int idCourt { get; set; }
        public string name { get; set; } = null!;
        public bool isAvailable { get; set; }
    }
}