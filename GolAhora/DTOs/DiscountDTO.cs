using System.ComponentModel.DataAnnotations;

namespace GolAhora.DTOs
{
    public class DiscountDTO
    {
        public string Name { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public double DiscountValue { get; set; }
        public string Conditions { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DiscountResponseDTO
    {
        public int IdDiscount { get; set; }
        public string Name { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public double DiscountValue { get; set; }
    }

    public class DiscountDetailDTO
    {
        public int IdDiscount { get; set; }
        public string Name { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public double DiscountValue { get; set; }
        public string Conditions { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}