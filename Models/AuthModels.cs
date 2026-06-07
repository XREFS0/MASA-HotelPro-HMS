namespace MasaHotelPro.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } // Admin, Receptionist, Accountant, Housekeeping
    
    public ICollection<User> Users { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
}
