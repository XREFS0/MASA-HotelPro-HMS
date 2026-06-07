namespace MasaHotelPro.Models;

public class MaintenanceTask
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public string IssueDescription { get; set; }
    public string Status { get; set; } // Pending, In Progress, Completed
    public int AssignedEmployeeId { get; set; }
    public Employee AssignedEmployee { get; set; }
    public decimal RepairCost { get; set; }
}
