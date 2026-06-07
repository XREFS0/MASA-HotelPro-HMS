using MasaHotelPro.Models;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MasaHotelPro.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;

    public SettingsService()
    {
        // Save in the AppData folder to ensure write permissions
        var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "MasaHotelPro");
        Directory.CreateDirectory(appFolder); // Ensure directory exists
        
        _settingsFilePath = Path.Combine(appFolder, "hotel_settings.json");
    }

    public async Task<HotelSettings> LoadSettingsAsync()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new HotelSettings(); // Return defaults
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFilePath);
            return JsonSerializer.Deserialize<HotelSettings>(json) ?? new HotelSettings();
        }
        catch
        {
            return new HotelSettings();
        }
    }

    public async Task SaveSettingsAsync(HotelSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFilePath, json);
    }
}
