namespace GolAhora.DTOs;

public class ReceiptDTO
{
    public int IdPayment { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public double TotalAmount { get; set; }
    public string Details { get; set; } = null!;
    public DateTime? Date { get; set; } 
}

public class ReceiptResponseDTO
{
    public int IdReceipt { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public double TotalAmount { get; set; }
    public DateTime Date { get; set; }
}

public class ReceiptDetailDTO
{
    public int IdReceipt { get; set; }
    public int IdPayment { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public double TotalAmount { get; set; }
    public string Details { get; set; } = null!;
    public DateTime Date { get; set; }
}