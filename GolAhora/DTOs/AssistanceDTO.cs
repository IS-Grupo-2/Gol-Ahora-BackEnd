using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolAhora.DTOs

{
    public class AssistanceDTO
    {
        public int clienteId { get; set; }
        public int classId { get; set; }
        public bool isAssisted { get; set; }
        public string observations { get; set; } = null!;
    }
    public class AssistanceResponseDTO
    {
        public int idAssistance { get; set; }
        public int clienteId { get; set; }
        public int classId { get; set; }
        public bool isAssisted { get; set; }
        public string observations { get; set; } = null!;
    }
    public class AssistanceDetailDTO
    {
        public int idAssistance { get; set; }
        public int clienteId { get; set; }
        public int classId { get; set; }
        public bool isAssisted { get; set; }
        public string observations { get; set; } = null!;
    }

}
