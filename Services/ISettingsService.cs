using MasaHotelPro.Models;
using System.Threading.Tasks;

namespace MasaHotelPro.Services;

public interface ISettingsService
{
    Task<HotelSettings> LoadSettingsAsync();
    Task SaveSettingsAsync(HotelSettings settings);
}
