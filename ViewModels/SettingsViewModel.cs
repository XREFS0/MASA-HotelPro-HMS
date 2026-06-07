using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MasaHotelPro.Messages;
using MasaHotelPro.Models;
using MasaHotelPro.Services;
using System.Threading.Tasks;
using System.Windows;

namespace MasaHotelPro.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _hotelName = string.Empty;

    [ObservableProperty]
    private string _currency = string.Empty;

    [ObservableProperty]
    private double _taxRate;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();
        HotelName = settings.HotelName;
        Currency = settings.Currency;
        TaxRate = settings.TaxRate;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (string.IsNullOrWhiteSpace(HotelName))
        {
            MessageBox.Show("Hotel Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var settings = new HotelSettings
        {
            HotelName = HotelName,
            Currency = Currency,
            TaxRate = TaxRate
        };

        await _settingsService.SaveSettingsAsync(settings);
        
        // Notify other ViewModels
        WeakReferenceMessenger.Default.Send(new SettingsChangedMessage(settings));

        MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
