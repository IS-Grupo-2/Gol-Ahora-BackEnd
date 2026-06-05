using System;

namespace GolAhora.DTOs
{
    public class AssistanceDTO
    {
        public int idClient { get; set; }
        public int classId { get; set; }
        public bool isAssisted { get; set; }
        public string observations { get; set; } = string.Empty;
    }

    public class AssistanceResponseDTO
    {
        public int idAssistance { get; set; }
        public int idClient { get; set; }
        public int classId { get; set; }
        public string clientFullName { get; set; } = null!;
        public DateTime date { get; set; }
        public bool isAssisted { get; set; }
        public string observations { get; set; } = string.Empty;
    }
}
