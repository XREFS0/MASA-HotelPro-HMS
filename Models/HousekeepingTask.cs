namespace MasaHotelPro.Models;

public class HousekeepingTask
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } // Pending, In Progress, Completed
}
