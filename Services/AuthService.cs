using MasaHotelPro.Models;
using MasaHotelPro.Repositories;
using System.Linq;

namespace MasaHotelPro.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    
    public User CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public AuthService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> LoginAsync(string username, string password)
    {
        // In a real application, you would hash the password and compare hashes
        var users = await _userRepository.FindAsync(u => u.Username == username && u.PasswordHash == password);
        var user = users.FirstOrDefault();
        
        if (user != null)
        {
            CurrentUser = user;
        }
        
        return user;
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        return Task.CompletedTask;
    }
}
