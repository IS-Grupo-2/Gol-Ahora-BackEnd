// CourtDTO.cs
namespace GolAhora.DTOs
{
    public class CourtDTO
    {
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public string imageUrl { get; set; } = null!;
        public int courtTypeId { get; set; }
    }
}