namespace MasaHotelPro.Models;

public class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; } // Single Room, Double Room, etc.
    public string Description { get; set; }
    public int Capacity { get; set; } // Number of persons
    public decimal BasePrice { get; set; }

    public ICollection<Room> Rooms { get; set; }
}
