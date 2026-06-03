using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolAhora.DTOs
{
    public class PaymentDTO
    {
        public int idClient { get; set; }
        public double amount { get; set; }
        public DateTime paymentDate { get; set; }
        public string paymentMethod { get; set; } = null!;
        public bool isSuccessful { get; set; }
        public int? idDiscount { get; set; }
        public int idReservation { get; set; }
    }
        public class PaymentResponseDTO
        {
            public int idPayment { get; set; }
            public int idClient { get; set; }
            public double amount { get; set; }
            public DateTime paymentDate { get; set; }
            public string paymentMethod { get; set; } = null!;
            public bool isSuccessful { get; set; }
        }
        public class PaymentDetailDTO
        {
            public int idPayment { get; set; }
            public int idClient { get; set; }
            public double amount { get; set; }
            public DateTime paymentDate { get; set; }
            public string paymentMethod { get; set; } = null!;
            public bool isSuccessful { get; set; }
            public int? idDiscount { get; set; }
        }
 }


