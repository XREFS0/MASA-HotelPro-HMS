using MasaHotelPro.Models;

namespace MasaHotelPro.Services;

public interface IAuthService
{
    Task<User> LoginAsync(string username, string password);
    Task LogoutAsync();
    User CurrentUser { get; }
    bool IsLoggedIn { get; }
}
