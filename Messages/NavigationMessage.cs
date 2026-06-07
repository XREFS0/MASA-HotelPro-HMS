using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MasaHotelPro.Messages;

public class NavigationMessage : ValueChangedMessage<string>
{
    public NavigationMessage(string viewName) : base(viewName)
    {
    }
}
