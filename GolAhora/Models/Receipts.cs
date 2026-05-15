namespace GolAhora.Models
{
    public class Receipts
    {
        public int idReceipt { get; set; }
        public int idPayment { get; set; }
        public Payments payment { get; set; } = null!;
        public DateTime date { get; set; }
        public string receiptNumber { get; set; } = null!;
        public double totalAmount { get; set; }
        public string details { get; set; } = null!;
    }
}
