namespace MasaHotelPro.Models;

public class Guest
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Nationality { get; set; }
    public string? DocumentNumber { get; set; } // ID or Passport
    public string? Address { get; set; }
    public string? Notes { get; set; }

    public ICollection<Reservation> Reservations { get; set; }
}
