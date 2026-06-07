using MasaHotelPro.Models;

namespace MasaHotelPro.Messages;

public class SettingsChangedMessage
{
    public HotelSettings NewSettings { get; }

    public SettingsChangedMessage(HotelSettings newSettings)
    {
        NewSettings = newSettings;
    }
}
