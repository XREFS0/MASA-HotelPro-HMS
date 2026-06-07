namespace MasaHotelPro.Models;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; }
    public decimal AccommodationTotal { get; set; }
    public decimal ServicesTotal { get; set; }
    public decimal Taxes { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime IssueDate { get; set; }
    public string Status { get; set; } = "Unpaid";

    public ICollection<Payment> Payments { get; set; }
}
