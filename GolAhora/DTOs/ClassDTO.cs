using GolAhora.Models;
using System;

namespace GolAhora.DTOs
{
    public class ClassDTO
    {
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public string classType { get; set; } = null!;
        public int profesorId { get; set; }
        public int courtId { get; set; }
        public DateTime date { get; set; }
        public int capacityMax { get; set; }
        public int duration { get; set; }
        public double price { get; set; }
    }

    public class ClassResponseDTO
    {
        public int idClass { get; set; }
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public string classType { get; set; } = null!;
        public int profesorId { get; set; }
        public string professorFullName { get; set; } = null!;
        public int courtId { get; set; }
        public string courtName { get; set; } = null!;
        public DateTime date { get; set; }
        public int capacityMax { get; set; }
        public int currentAlumnosCount { get; set; }
        public int duration { get; set; }
        public double price { get; set; }
        public bool isActive { get; set; }
    }
}