using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MasaHotelPro.Data;
using MasaHotelPro.Repositories;
using MasaHotelPro.Models;
using MasaHotelPro.ViewModels;
using System.Threading.Tasks;

class Program {
    static async Task Main() {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=masahotelpro.db"));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient<GuestsViewModel>();
        var sp = services.BuildServiceProvider();
        
        var vm = sp.GetRequiredService<GuestsViewModel>();
        // Wait for LoadDataAsync (which is fire-and-forget, so delay)
        await Task.Delay(1000);
        
        vm.AddGuestCommand.Execute(null);
        vm.GuestForm.Name = "Test Crasher";
        
        Console.WriteLine("Executing SaveGuestCommand...");
        try {
            vm.SaveGuestCommand.Execute(null);
            await Task.Delay(1000);
            Console.WriteLine("Save completed successfully without crashing.");
        } catch (Exception ex) {
            Console.WriteLine("CAUGHT EXCEPTION: " + ex.ToString());
        }
    }
}
