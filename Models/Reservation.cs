namespace MasaHotelPro.Models;

public enum ReservationStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    CheckedOut,
    Cancelled
}

public class Reservation
{
    public int Id { get; set; }
    public string ReservationNumber { get; set; }
    public int GuestId { get; set; }
    public Guest Guest { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfNights { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    
    public ICollection<Invoice> Invoices { get; set; }
}
