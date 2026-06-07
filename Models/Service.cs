namespace MasaHotelPro.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } // Restaurant, Laundry, Spa, Gym, Airport Transfer
    public decimal Price { get; set; }
    public string Description { get; set; }
}
