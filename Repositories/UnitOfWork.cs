using MasaHotelPro.Data;
using MasaHotelPro.Models;

namespace MasaHotelPro.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IRepository<Room> Rooms { get; private set; }
    public IRepository<Guest> Guests { get; private set; }
    public IRepository<Reservation> Reservations { get; private set; }
    public IRepository<Invoice> Invoices { get; private set; }
    public IRepository<Employee> Employees { get; private set; }
    public IRepository<HousekeepingTask> HousekeepingTasks { get; private set; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Rooms = new Repository<Room>(_context);
        Guests = new Repository<Guest>(_context);
        Reservations = new Repository<Reservation>(_context);
        Invoices = new Repository<Invoice>(_context);
        Employees = new Repository<Employee>(_context);
        HousekeepingTasks = new Repository<HousekeepingTask>(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
