using System.ComponentModel.DataAnnotations;

namespace GolAhora.DTOs
{
    public class DisponibilityDTO
    {
        public DayOfWeek day { get; set; }
        [Range(typeof(TimeSpan), "00:00:00", "23:59:59", ErrorMessage = "El valor no puede exceder las 24 horas o respte este formato: hh:mm.")]
        public TimeSpan startTime { get; set; }
        [Range(typeof(TimeSpan), "00:00:00", "23:59:59", ErrorMessage = "El valor no puede exceder las 24 horas o respte este formato: hh:mm.")]
        public TimeSpan endTime { get; set; }
        public bool isAvailable { get; set; }
        public int courtId { get; set; }
    }
    
}