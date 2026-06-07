using System.Configuration;
using System.Data;
using System.Windows;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace MasaHotelPro;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider Services { get; }

    public new static App Current => (App)Application.Current;

    public App()
    {
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Database
        services.AddDbContext<Data.AppDbContext>(options =>
        {
            options.UseSqlite("Data Source=masahotelpro.db");
        });

        // Repositories
        services.AddScoped(typeof(Repositories.IRepository<>), typeof(Repositories.Repository<>));
        services.AddScoped<Repositories.IUnitOfWork, Repositories.UnitOfWork>();

        // Services
        services.AddSingleton<Services.IAuthService, Services.AuthService>();
        services.AddSingleton<Services.ISettingsService, Services.SettingsService>();

        // ViewModels
        services.AddTransient<ViewModels.MainViewModel>();
        services.AddTransient<ViewModels.DashboardViewModel>();
        services.AddTransient<ViewModels.RoomsViewModel>();
        services.AddTransient<ViewModels.GuestsViewModel>();
        services.AddTransient<ViewModels.EmployeesViewModel>();
        services.AddTransient<ViewModels.ReservationsViewModel>();
        services.AddTransient<ViewModels.BillingViewModel>();
        services.AddTransient<ViewModels.ReportsViewModel>();
        services.AddTransient<ViewModels.HousekeepingViewModel>();
        services.AddTransient<ViewModels.SettingsViewModel>();

        // Views
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splashScreen = new Views.SplashWindow();
        splashScreen.Show();

        // Give the UI thread a moment to render the splash screen
        await Task.Delay(100);

        try
        {
            // Initialize database asynchronously
            await Task.Run(() => 
            {
                using var scope = Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
                dbContext.Database.EnsureCreated();
                
                // Add an artificial delay to show off the splash screen
                Task.Delay(2500).Wait();
            });

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = Services.GetRequiredService<ViewModels.MainViewModel>();
            
            splashScreen.Close();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start application: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
