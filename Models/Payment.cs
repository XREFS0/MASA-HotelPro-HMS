namespace MasaHotelPro.Models;

public enum PaymentMethod
{
    Cash,
    Visa,
    MasterCard,
    BankTransfer,
    OnlinePayment
}

public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; }
}
