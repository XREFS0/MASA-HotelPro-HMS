namespace MasaHotelPro.Models;

public enum RoomStatus
{
    Available,
    Occupied,
    Reserved,
    Cleaning,
    Maintenance
}

public class Room
{
    public int Id { get; set; }
    public string RoomNumber { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; } // Specific price or override
    public RoomStatus Status { get; set; }
}
