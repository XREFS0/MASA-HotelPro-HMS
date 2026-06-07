namespace MasaHotelPro.Repositories;

public interface IUnitOfWork : IDisposable
{
    // Expose specific repositories if needed or generic ones
    IRepository<Models.Room> Rooms { get; }
    IRepository<Models.Guest> Guests { get; }
    IRepository<Models.Reservation> Reservations { get; }
    IRepository<Models.Invoice> Invoices { get; }
    IRepository<Models.Employee> Employees { get; }
    IRepository<Models.HousekeepingTask> HousekeepingTasks { get; }
    // ... add others as needed

    Task<int> CompleteAsync();
}
