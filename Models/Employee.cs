namespace MasaHotelPro.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string JobTitle { get; set; } // Manager, Receptionist, Accountant, Room Service
    public decimal Salary { get; set; }
    public string Phone { get; set; }
}
