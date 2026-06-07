using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MasaHotelPro.Messages;
using MasaHotelPro.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MasaHotelPro.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _windowTitle = "MASA HotelPro HMS";

    [ObservableProperty]
    private object _currentView;

    private readonly IServiceProvider _serviceProvider;
    private readonly ISettingsService _settingsService;

    [ObservableProperty] private int _sidebarWidth = 260;
    [ObservableProperty] private bool _isSidebarCollapsed = false;

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
        SidebarWidth = IsSidebarCollapsed ? 80 : 260;
    }

    public MainViewModel(IServiceProvider serviceProvider, ISettingsService settingsService)
    {
        _serviceProvider = serviceProvider;
        _settingsService = settingsService;
        
        _ = LoadInitialSettingsAsync();

        WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, (r, m) =>
        {
            WindowTitle = $"{m.NewSettings.HotelName} HMS";
        });

        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
        {
            Navigate(m.Value);
        });

        // Start with Dashboard
        Navigate("Dashboard");
    }

    private async Task LoadInitialSettingsAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();
        WindowTitle = $"{settings.HotelName} HMS";
    }

    [RelayCommand]
    private void Navigate(string viewName)
    {
        switch (viewName)
        {
            case "Dashboard":
                CurrentView = _serviceProvider.GetService(typeof(DashboardViewModel));
                break;
            case "Rooms":
                CurrentView = _serviceProvider.GetService(typeof(RoomsViewModel));
                break;
            case "Guests":
                CurrentView = _serviceProvider.GetService(typeof(GuestsViewModel));
                break;
            case "Employees":
                CurrentView = _serviceProvider.GetService(typeof(EmployeesViewModel));
                break;
            case "Reservations":
                CurrentView = _serviceProvider.GetService(typeof(ReservationsViewModel));
                break;
            case "Billing":
                CurrentView = _serviceProvider.GetService(typeof(BillingViewModel));
                break;
            case "Reports":
                CurrentView = _serviceProvider.GetService(typeof(ReportsViewModel));
                break;
            case "Housekeeping":
                CurrentView = _serviceProvider.GetService(typeof(HousekeepingViewModel));
                break;
            case "Settings":
                CurrentView = _serviceProvider.GetService(typeof(SettingsViewModel));
                break;
            // Add other cases as needed
        }
    }
}

